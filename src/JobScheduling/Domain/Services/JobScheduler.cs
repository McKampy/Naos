﻿namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class JobScheduler : IJobScheduler
    {
        private readonly ILogger<JobScheduler> logger;
        private readonly IMutex mutex;
        private int activeCount;
        private Action<Exception> errorHandler;

        public JobScheduler(ILoggerFactory loggerFactory, IMutex mutex)
            : this(loggerFactory, mutex, null)
        {
        }

        public JobScheduler(ILoggerFactory loggerFactory, IMutex mutex, JobSchedulerOptions options)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(options, nameof(options));

            this.logger = loggerFactory.CreateLogger<JobScheduler>();
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

            registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(job);
            this.logger.LogInformation($"{{LogKey:l}} registration (key={{JobKey}}, id={registration.Identifier}, cron={registration.Cron}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")}, enabled={registration.Enabled})", LogKeys.JobScheduling, registration.Key);

            var item = this.Options.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(registration.Key));
            if(item.Key != null)
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
            if(registration != null)
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
            if(item.Key != null)
            {
                await this.TriggerAsync(key, new CancellationTokenSource(item.Key.Timeout).Token, args);
            }
            else
            {
                this.logger.LogInformation("{LogKey:l} does not know registration with key {JobKey} registered", LogKeys.JobScheduling, key);
            }
        }

        public async Task TriggerAsync(string key, CancellationToken cancellationToken, string[] args = null)
        {
            var item = this.Options.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if(item.Key != null)
            {
                await this.ExecuteJobAsync(item.Key, item.Value, cancellationToken, args ?? item.Key?.Args).AnyContext();
            }
            else
            {
                this.logger.LogInformation("{LogKey:l} does not know registration with key {JobKey} registered", LogKeys.JobScheduling, key);
            }
        }

        public async Task RunAsync() // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            await this.RunAsync(DateTime.UtcNow);
        }

        public async Task RunAsync(DateTime moment) // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            if(!this.Options.Enabled)
            {
                //this.logger.LogDebug($"job scheduler run not started (enabled={this.Settings.Enabled})");
                return;
            }

            Interlocked.Increment(ref this.activeCount);
            this.logger.LogInformation($"{{LogKey:l}} run started (activeCount=#{this.activeCount}, moment={moment.ToString("o")})", LogKeys.JobScheduling);
            await this.ExecuteJobsAsync(moment).AnyContext();
            Interlocked.Decrement(ref this.activeCount);
            this.logger.LogInformation($"{{LogKey:l}} run finished (activeCount=#{this.activeCount})", LogKeys.JobScheduling);
        }

        private async Task ExecuteJobsAsync(DateTime moment)
        {
            var dueJobs = this.Options.Registrations.Where(r => r.Key?.IsDue(moment) == true && r.Key.Enabled).Select(r =>
            {
                var cts = new CancellationTokenSource(r.Key.Timeout);
                return Task.Run(async () =>
                {
                    await this.ExecuteJobAsync(r.Key, r.Value, cts.Token, r.Key.Args).AnyContext();
                }, cts.Token);
            }).ToList();

            if(dueJobs.IsNullOrEmpty())
            {
                this.logger.LogInformation($"{{LogKey:l}} run has no due jobs at moment {moment.ToString("o")}", LogKeys.JobScheduling);
            }

            await Task.WhenAll(dueJobs).AnyContext(); // really wait for completion (await)?
        }

        private async Task ExecuteJobAsync(JobRegistration registration, IJob job, CancellationToken cancellationToken, string[] args = null)
        {
            if(registration?.Key.IsNullOrEmpty() == false && job != null)
            {
                try
                {
                    async Task Execute()
                    {
                        using(var timer = new Foundation.Timer())
                        using(this.logger.BeginScope(new Dictionary<string, object>
                        {
                            [LogEventPropertyKeys.CorrelationId] = IdGenerator.Instance.Next
                        }))
                        {
                            // TODO: publish domain event (job started)
                            var span = IdGenerator.Instance.Next;
                            this.logger.LogJournal(LogKeys.JobScheduling, $"job started (key={{JobKey}}, id={registration.Identifier}, type={job.GetType().PrettyName()}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")})", LogEventPropertyKeys.TrackStartJob, args: new[] { registration.Key });
                            this.logger.LogTraceEvent(LogKeys.JobScheduling, span, registration.Key, LogTraceEventNames.Job);
                            await job.ExecuteAsync(cancellationToken, args).AnyContext();
                            await Run.DelayedAsync(new TimeSpan(0, 0, 1), () =>
                            {
                                timer.Stop();
                                this.logger.LogJournal(LogKeys.JobScheduling, $"job finished (key={{JobKey}}, id={registration.Identifier}, type={job.GetType().PrettyName()})", LogEventPropertyKeys.TrackFinishJob, args: new[] { LogKeys.JobScheduling, registration.Key });
                                this.logger.LogTraceEvent(LogKeys.JobScheduling, span, registration.Key, LogTraceEventNames.Job, timer.Elapsed);
                                return Task.CompletedTask;
                            });
                            // TODO: publish domain event (job finished)
                        }
                    }

                    if(!registration.IsReentrant)
                    {
                        if(this.mutex.TryAcquireLock(registration.Key))
                        {
                            try
                            {
                                await Execute();
                            }
                            finally
                            {
                                this.mutex.ReleaseLock(registration.Key);
                            }
                        }
                        else
                        {
                            this.logger.LogWarning($"{{LogKey:l}} already executing (key={{JobKey}}, type={job.GetType().PrettyName()})", LogKeys.JobScheduling, registration.Key);
                        }
                    }
                    else
                    {
                        await Execute();
                    }
                }
                catch(OperationCanceledException ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogWarning(ex, $"{{LogKey:l}} canceled (key={{JobKey}}), type={job.GetType().PrettyName()})", LogKeys.JobScheduling, registration.Key);
                    //this.errorHandler?.Invoke(ex);
                }
                catch(Exception ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogError(ex.InnerException ?? ex, $"{{LogKey:l}} failed (key={{JobKey}}), type={job.GetType().PrettyName()})", LogKeys.JobScheduling, registration.Key);
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