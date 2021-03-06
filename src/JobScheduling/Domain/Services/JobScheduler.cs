﻿namespace Naos.JobScheduling.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class JobScheduler : IJobScheduler
    {
        private readonly ILogger<JobScheduler> logger;
        private readonly ITracer tracer;
        private readonly IMutex mutex;
        private int activeCount;
        private Action<Exception> errorHandler;

        public JobScheduler(ILoggerFactory loggerFactory, ITracer tracer, IMutex mutex)
            : this(loggerFactory, tracer, mutex, null)
        {
        }

        public JobScheduler(ILoggerFactory loggerFactory, ITracer tracer, IMutex mutex, JobSchedulerOptions options)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            //EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNull(options, nameof(options));

            this.logger = loggerFactory.CreateLogger<JobScheduler>();
            this.tracer = tracer;
            this.mutex = mutex ?? new InProcessMutex(null);
            this.Options = options;
        }

        public bool IsRunning => this.activeCount > 0;

        public JobSchedulerOptions Options { get; }

        public IJobScheduler Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));
            EnsureArg.IsNotNull(job, nameof(job));

            registration.Key ??= HashAlgorithm.ComputeHash(job);
            this.logger.LogInformation($"{{LogKey:l}} registration (key={{JobKey:l}}, id={registration.Identifier}, cron={registration.Cron}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout:c}, enabled={registration.Enabled})", LogKeys.JobScheduling, registration.Key);

            var item = this.Options.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(registration.Key));
            if (item.Key != null)
            {
                this.Options.Registrations.Remove(item.Key);
            }

            this.Options.Registrations.Add(registration, job);
            return this;
        }

        public IJobScheduler UnRegister(string key)
        {
            return this.UnRegister(this.GetRegistationByKey(key));
        }

        public IJobScheduler UnRegister(JobRegistration registration)
        {
            if (registration != null)
            {
                this.Options.Registrations.Remove(registration);
            }

            return this;
        }

        public IJobScheduler OnError(Action<Exception> errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        public async Task TriggerAsync(string key, string[] args = null)
        {
            var item = this.Options.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                using (var cts = new CancellationTokenSource(item.Key.Timeout))
                {
                    await this.TriggerAsync(key, cts.Token, args).AnyContext();
                }
            }
            else
            {
                this.logger.LogInformation("{LogKey:l} unknown registration with key {JobKey:l} ", LogKeys.JobScheduling, key);
            }
        }

        public async Task TriggerAsync(string key, CancellationToken cancellationToken, string[] args = null)
        {
            var item = this.Options.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.ExecuteJobAsync(item.Key, item.Value, cancellationToken, args ?? item.Key?.Args).AnyContext();
            }
            else
            {
                this.logger.LogInformation("{LogKey:l} unknown registration with key {JobKey:l}", LogKeys.JobScheduling, key);
            }
        }

        public async Task RunAsync() // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            await this.RunAsync(DateTime.UtcNow).AnyContext();
        }

        public async Task RunAsync(DateTime moment) // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            if (!this.Options.Enabled)
            {
                //this.logger.LogDebug($"job scheduler run not started (enabled={this.Settings.Enabled})");
                return;
            }

            Interlocked.Increment(ref this.activeCount);
            await this.ExecuteJobsAsync(moment).AnyContext();
            Interlocked.Decrement(ref this.activeCount);
        }

        private async Task ExecuteJobsAsync(DateTime moment)
        {
            var dueJobs = this.Options.Registrations
                .Where(r => r.Key?.IsDue(moment) == true && r.Key.Enabled)
                .Select(r =>
                {
                    var cts = new CancellationTokenSource(r.Key.Timeout);
                    return Task.Run(async () =>
                        await this.ExecuteJobAsync(r.Key, r.Value, cts.Token, r.Key.Args).AnyContext(),
                        cts.Token);
                }).ToList();

            if (dueJobs.IsNullOrEmpty())
            {
                this.logger.LogDebug($"{{LogKey:l}} run has no due jobs, not starting (activeCount=#{this.activeCount}, moment={moment:o})", LogKeys.JobScheduling);
            }
            else
            {
                this.logger.LogInformation($"{{LogKey:l}} run started (activeCount=#{this.activeCount}, moment={moment:o})", LogKeys.JobScheduling);
                await Task.WhenAll(dueJobs).AnyContext(); // really wait for completion (await)?
                this.logger.LogInformation($"{{LogKey:l}} run finished (activeCount=#{this.activeCount})", LogKeys.JobScheduling);
            }
        }

        private async Task ExecuteJobAsync(JobRegistration registration, IJob job, CancellationToken cancellationToken, string[] args = null)
        {
            if (registration?.Key.IsNullOrEmpty() == false && job != null)
            {
                try
                {
                    async Task ExecuteAsync()
                    {
                        var correlationId = IdGenerator.Instance.Next;
                        using (var timer = new Foundation.Timer())
                        using (this.logger.BeginScope(new Dictionary<string, object>
                        {
                            [LogPropertyKeys.CorrelationId] = correlationId
                        }))
                        {
                            // TODO: publish domain event (job started)
                            this.logger.LogJournal(LogKeys.JobScheduling, $"job started: {{JobKey:l}} (id={registration.Identifier}, type={job.GetType().PrettyName()}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout:c})", LogPropertyKeys.TrackStartJob, args: new[] { registration.Key });
                            //using (var scope = this.tracer?.BuildSpan($"job run {registration.Key}", LogKeys.JobScheduling, SpanKind.Producer).Activate(this.logger))
                            //{ // current span is somehow not available in created jobs (ServiceProviderJobFactory)
                            try
                            {
                                await job.ExecuteAsync(correlationId, cancellationToken, args).AnyContext();
                                this.logger.LogJournal(LogKeys.JobScheduling, $"job finished: {{JobKey:l}} (id={registration.Identifier}, type={job.GetType().PrettyName()})", LogPropertyKeys.TrackFinishJob, args: new[] { registration.Key });
                            }
                            catch (Exception ex)
                            {
                                this.logger.LogError(ex, $"{{LogKey:l}} job failed: {{JobKey:l}} (id={registration.Identifier}, type={job.GetType().PrettyName()}) {ex.GetFullMessage()}", args: new[] { LogKeys.JobScheduling, registration.Key });
                            }

                            // TODO: publish domain event (job finished)
                        }
                    }

                    if (!registration.IsReentrant)
                    {
                        if (this.mutex.TryAcquireLock(registration.Key))
                        {
                            try
                            {
                                await ExecuteAsync().AnyContext();
                            }
                            finally
                            {
                                this.mutex.ReleaseLock(registration.Key);
                            }
                        }
                        else
                        {
                            this.logger.LogWarning($"{{LogKey:l}} already executing (key={{JobKey:l}}, type={job.GetType().PrettyName()})", LogKeys.JobScheduling, registration.Key);
                        }
                    }
                    else
                    {
                        await ExecuteAsync().AnyContext();
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogWarning(ex, $"{{LogKey:l}} canceled (key={{JobKey:l}}), type={job.GetType().PrettyName()})", LogKeys.JobScheduling, registration.Key);
                    //this.errorHandler?.Invoke(ex);
                }
                catch (Exception ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogError(ex.InnerException ?? ex, $"{{LogKey:l}} failed (key={{JobKey:l}}), type={job.GetType().PrettyName()})", LogKeys.JobScheduling, registration.Key);
                    this.errorHandler?.Invoke(ex.InnerException ?? ex);
                }
            }
        }

        private JobRegistration GetRegistationByKey(string key)
        {
            return this.Options.Registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
        }
    }
}