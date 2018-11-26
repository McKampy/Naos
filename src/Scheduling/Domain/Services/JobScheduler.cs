﻿namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class JobScheduler : IJobScheduler
    {
        private readonly Dictionary<JobRegistration, IJob> registrations = new Dictionary<JobRegistration, IJob>();
        private readonly ILogger<JobScheduler> logger;
        private readonly IMutex mutex;
        private readonly IJobFactory jobFactory;
        private int activeCount;
        private Action<Exception> errorHandler;

        public JobScheduler(ILogger<JobScheduler> logger, IJobFactory jobFactory, IMutex mutex)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.mutex = mutex ?? new InProcessMutex();
            this.jobFactory = jobFactory; // what to do when null?
        }

        public bool IsRunning => this.activeCount > 0;

        public IJobScheduler Register(string cron, Action<string[]> action)
        {
            return this.Register(new JobRegistration(null, cron), new Job(action));
        }

        public IJobScheduler Register(string key, string cron, Action<string[]> action)
        {
            return this.Register(new JobRegistration(key, cron), new Job(action));
        }

        public IJobScheduler Register(string cron, Func<string[], Task> task)
        {
            return this.Register(new JobRegistration(null, cron), new Job(task));
        }

        public IJobScheduler Register(string key, string cron, Func<string[], Task> task)
        {
            return this.Register(new JobRegistration(key, cron), new Job(task));
        }

        public IJobScheduler Register<T>(string cron, string[] args = null)
            where T : IJob
        {
            return this.Register<T>(null, cron);
        }

        public IJobScheduler Register<T>(string key, string cron, string[] args = null)
            where T : IJob
        {
            if (!typeof(Job).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Job type to register must implement IJob.");
            }

            return this.Register(
                new JobRegistration(key, cron),
                new Job(async (a) => // defer job creation
                {
                    var job = this.jobFactory.Create(typeof(T));
                    if(job == null)
                    {
                        throw new NaosException($"Cannot create job instance for type {typeof(T).PrettyName()}.");
                    }

                    await job.ExecuteAsync(a).ConfigureAwait(false);
                }));
        }

        public IJobScheduler Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));

            if (job != null)
            {
                registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(job);
                this.logger.LogInformation($"register scheduled job (key={registration.Key}, cron={registration.Cron})");
                this.registrations.Add(registration, job); // TODO: remove existing by key
            }

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
                this.registrations.Remove(registration);
            }

            return this;
        }

        public IJobScheduler OnError(Action<Exception> errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        public async Task TriggerAsync(string key)
        {
            var item = this.registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.ExecuteJobAsync(item.Key, item.Value).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogInformation($"job scheduler does not have a job registered with key {key}");
            }
        }

        public async Task RunAsync()
        {
            await this.RunAsync(DateTime.UtcNow);
        }

        public async Task RunAsync(DateTime moment)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            Interlocked.Increment(ref this.activeCount);
            this.logger.LogInformation($"job scheduler run started (activeCount=#{this.activeCount}, moment={moment.ToString("o")})");
            await this.ExecuteJobsAsync(moment).ConfigureAwait(false);
            Interlocked.Decrement(ref this.activeCount);
            this.logger.LogInformation($"job scheduler run finished (activeCount=#{this.activeCount})");
        }

private async Task ExecuteJobsAsync(DateTime moment)
        {
            var dueJobs = this.registrations.Where(t => t.Key?.IsDue(moment) == true).Select(t =>
            {
                return Task.Run(() =>
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.ExecuteJobAsync(t.Key, t.Value); // dont use await for better parallism
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                });
            }).ToList();

            if (dueJobs.IsNullOrEmpty())
            {
                this.logger.LogInformation($"job scheduler run no due jobs at moment {moment.ToString("o")}");
            }

            await Task.WhenAll(dueJobs).ConfigureAwait(false); // really wait for completion (await)?
        }

        private async Task ExecuteJobAsync(JobRegistration registration, IJob job)
        {
            if (registration?.Key.IsNullOrEmpty() == false && job != null)
            {
                try
                {
                    async Task Execute()
                    {
                        // TODO: publish domain event (task started)
                        this.logger.LogInformation($"job scheduled job started (key={registration.Key}, type={job.GetType().PrettyName()})");
                        await job.ExecuteAsync().ConfigureAwait(false);
                        this.logger.LogInformation($"job scheduled job finished (key={registration.Key}, type={job.GetType().PrettyName()})");
                        // TODO: publish domain event (job finished)
                    }

                    if (registration.PreventOverlap)
                    {
                        if (this.mutex.TryAcquireLock(registration.Key))
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
                            this.logger.LogWarning($"job scheduled already executing (key={registration.Key}, type={job.GetType().PrettyName()})");
                        }
                    }
                    else
                    {
                        await Execute();
                    }
                }
                catch (Exception ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogError(ex, $"job scheduled failed (key={registration.Key}), type={job.GetType().PrettyName()}) {ex.Message}");
                    this.errorHandler?.Invoke(ex);
                }
            }
        }

        private JobRegistration GetRegistationByKey(string key)
        {
            return this.registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
        }
    }
}
