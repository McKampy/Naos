﻿namespace Naos.Core.Queueing.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class AzureStorageQueue<TData> : BaseQueue<TData, AzureStorageQueueOptions>
        where TData : class
    {
        private readonly CloudQueue queue;
        private readonly CloudQueue deadletterQueue;
        private long enqueuedCount;
        private long dequeuedCount;
        private long completedCount;
        private long abandonedCount;
        private long workerErrorCount;
        private bool queueCreated;
        private bool isProcessing;

        public AzureStorageQueue()
            : this(o => o)
        {
        }

        public AzureStorageQueue(AzureStorageQueueOptions options)
            : base(options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));

            var account = CloudStorageAccount.Parse(options.ConnectionString);
            var client = account.CreateCloudQueueClient();
            if(options.RetryPolicy != null)
            {
                client.DefaultRequestOptions.RetryPolicy = options.RetryPolicy;
            }

            this.queue = client.GetQueueReference(this.options.Name);
            this.deadletterQueue = client.GetQueueReference($"{this.options.Name}-poison");
        }

        public AzureStorageQueue(Builder<AzureStorageQueueOptionsBuilder, AzureStorageQueueOptions> optionsBuilder)
            : this(optionsBuilder(new AzureStorageQueueOptionsBuilder()).Build())
        {
        }

        public override async Task<string> EnqueueAsync(TData data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            this.EnsureMetaData(data);

            using(this.logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = data.As<IHaveCorrelationId>()?.CorrelationId,
            }))
            {
                await this.EnsureQueueAsync().AnyContext();

                this.logger.LogDebug($"queue item enqueue (queue={this.options.Name})");

                Interlocked.Increment(ref this.enqueuedCount);
                var message = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(this.serializer.SerializeToBytes(data));
                // TODO: store correlationid?
                await this.queue.AddMessageAsync(message).AnyContext();

                this.logger.LogJournal(LogKeys.Queueing, $"item enqueued (id={message.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogPropertyKeys.TrackEnqueue);
                this.logger.LogTrace(LogKeys.Queueing, message.Id, typeof(TData).PrettyName(), LogTraceNames.Queue);
                this.LastEnqueuedDate = DateTime.UtcNow;
                return message.Id;
            }
        }

        public override async Task RenewLockAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogDebug($"queue item renew (id={item.Id}, queue={this.options.Name})");

            var message = this.ToMessage(item);
            await this.queue.UpdateMessageAsync(message, this.options.ProcessTimeout, MessageUpdateFields.Visibility).AnyContext();

            //this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item lock renewed (id={message.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task CompleteAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogDebug($"queue item complete (id={item.Id}, queue={this.options.Name})");

            if(item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            var message = this.ToMessage(item);
            await this.queue.DeleteMessageAsync(message).AnyContext();

            Interlocked.Increment(ref this.completedCount);
            item.MarkCompleted();

            this.logger.LogJournal(LogKeys.Queueing, $"item completed (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task AbandonAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogDebug($"queue item abandon (id={item.Id}, queue={this.options.Name})");

            if(item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            var message = this.ToMessage(item);
            if(message.DequeueCount > this.options.Retries)
            {
                // too many retries
                await Task.WhenAll(
                    this.queue.DeleteMessageAsync(message),
                    this.deadletterQueue.AddMessageAsync(message)).AnyContext();
            }
            else
            {
                await this.queue.UpdateMessageAsync(message, TimeSpan.Zero, MessageUpdateFields.Visibility).AnyContext(); // item available immediately
            }

            Interlocked.Increment(ref this.abandonedCount);
            item.MarkAbandoned();

            this.logger.LogJournal(LogKeys.Queueing, $"item abandoned (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task<QueueMetrics> GetMetricsAsync()
        {
            await this.EnsureQueueAsync().AnyContext();

            await Task.WhenAll(
                this.queue.FetchAttributesAsync(),
                this.deadletterQueue.FetchAttributesAsync()).AnyContext();

            return new QueueMetrics
            {
                Queued = this.queue.ApproximateMessageCount.GetValueOrDefault(),
                Working = 0,
                Deadlettered = this.deadletterQueue.ApproximateMessageCount.GetValueOrDefault(),
                Enqueued = this.enqueuedCount,
                Dequeued = this.dequeuedCount,
                Completed = this.completedCount,
                Abandoned = this.abandonedCount,
                Errors = this.workerErrorCount,
                Timeouts = 0
            };
        }

        public override async Task ProcessItemsAsync(Func<IQueueItem<TData>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            this.ProcessItems(handler, autoComplete, cancellationToken);
        }

        public override async Task ProcessItemsAsync(bool autoComplete, CancellationToken cancellationToken)
        {
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            if(this.options.Mediator == null)
            {
                throw new NaosException("queue processing error: no mediator instance provided");
            }

            if(!this.isProcessing)
            {
                this.ProcessItems(
                    async (i, ct) => await this.options.Mediator.Send(new QueueEvent<TData>(i), ct).AnyContext(),
                    autoComplete, cancellationToken);
                this.isProcessing = true;
            }
        }

        public override async Task DeleteQueueAsync()
        {
            await Task.WhenAll(
                this.queue.DeleteIfExistsAsync(),
                this.deadletterQueue.DeleteIfExistsAsync()).AnyContext();

            this.queueCreated = false;
            this.enqueuedCount = 0;
            this.dequeuedCount = 0;
            this.completedCount = 0;
            this.abandonedCount = 0;
            this.workerErrorCount = 0;
        }

        protected override async Task EnsureQueueAsync(CancellationToken cancellationToken = default)
        {
            if(this.queueCreated)
            {
                return;
            }

            await Task.WhenAll(
                this.queue.CreateIfNotExistsAsync(),
                this.deadletterQueue.CreateIfNotExistsAsync()).AnyContext();
            //Task.Delay(5000, cancellationToken).Wait(); // wait till created

            this.queueCreated = true;
        }

        protected override async Task<IQueueItem<TData>> DequeueWithIntervalAsync(CancellationToken cancellationToken)
        {
            await this.EnsureQueueAsync().AnyContext();
            this.logger.LogDebug($"queue item dequeue (queue={this.options.Name})");

            var message = await this.queue.GetMessageAsync(this.options.ProcessTimeout, null, null).AnyContext();
            if(message == null)
            {
                while(message == null && !cancellationToken.IsCancellationRequested)
                {
                    if(!cancellationToken.IsCancellationRequested)
                    {
                        Task.Delay(this.options.DequeueInterval.Milliseconds).Wait();
                    }

                    //try
                    //{
                    message = await this.queue.GetMessageAsync(this.options.ProcessTimeout, null, null).AnyContext();
                    //}
                    //catch (Exception ex)
                    //{
                    //    this.logger.LogError(ex, $"queue processing error: {ex.Message}");
                    //}
                }
            }

            if(message == null)
            {
                return null;
            }

            return this.HandleDequeue(message); // convert message to item
        }

        private void ProcessItems(Func<IQueueItem<TData>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            var linkedCancellationToken = this.CreateLinkedTokenSource(cancellationToken);

            Task.Run(async () =>
            {
                this.logger.LogInformation($"{{LogKey:l}} processing started (queue={this.options.Name}, type={this.GetType().PrettyName()})", args: new[] { LogKeys.Queueing });
                while(!linkedCancellationToken.IsCancellationRequested)
                {
                    IQueueItem<TData> item = null;
                    try
                    {
                        item = await this.DequeueWithIntervalAsync(linkedCancellationToken.Token).AnyContext();
                    }
                    catch(Exception ex)
                    {
                        this.logger.LogError(ex, $"{{LogKey:l}} processing error: {ex.Message}", args: new[] { LogKeys.Queueing });
                    }

                    if(linkedCancellationToken.IsCancellationRequested || item == null)
                    {
                        await Task.Delay(this.options.ProcessTimeout.Milliseconds);
                        continue;
                    }

                    using(this.logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogPropertyKeys.CorrelationId] = item.Data.As<IHaveCorrelationId>()?.CorrelationId,
                    }))
                    {
                        try
                        {
                            await handler(item, linkedCancellationToken.Token).AnyContext();
                            if(autoComplete && !item.IsAbandoned && !item.IsCompleted)
                            {
                                await item.CompleteAsync().AnyContext();
                            }
                        }
                        catch(Exception ex)
                        {
                            Interlocked.Increment(ref this.workerErrorCount);
                            this.logger.LogError(ex, $"{{LogKey:l}} processing error: {ex.Message}", args: new[] { LogKeys.Queueing });

                            if(!item.IsAbandoned && !item.IsCompleted)
                            {
                                await item.AbandonAsync().AnyContext();
                            }
                        }
                    }
                }

                this.logger.LogDebug($"queue processing exiting (name={this.options.Name}, cancellation={linkedCancellationToken.IsCancellationRequested})");
            }, linkedCancellationToken.Token)
                .ContinueWith(t => linkedCancellationToken.Dispose());
        }

        private IQueueItem<TData> HandleDequeue(CloudQueueMessage message)
        {
            Interlocked.Increment(ref this.dequeuedCount);

            var item = new AzureStorageQueueItem<TData>(
                message,
                this.serializer.Deserialize<TData>(message.AsBytes),
                this);

            using(this.logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = item.Data.As<IHaveCorrelationId>()?.CorrelationId,
            }))
            {
                this.logger.LogJournal(LogKeys.Queueing, $"item dequeued (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
                this.logger.LogTrace(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceNames.Queue, DateTime.UtcNow - item.EnqueuedDate);
                this.LastDequeuedDate = DateTime.UtcNow;
                return item;
            }
        }

        private CloudQueueMessage ToMessage(IQueueItem<TData> item)
        {
#pragma warning disable SA1119 // Statement must not use unnecessary parenthesis
            if(!(item is AzureStorageQueueItem<TData> azureQueueItem))
#pragma warning restore SA1119 // Statement must not use unnecessary parenthesis
            {
                throw new ArgumentException($"invalid queue item type, not of type '{nameof(AzureStorageQueueItem<TData>)}'");
            }

            return azureQueueItem.Message;
        }
    }
}