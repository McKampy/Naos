﻿namespace Naos.Core.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Scheduling.Domain;

    public class JobSchedulerSettings
    {
        private readonly IDictionary<JobRegistration, IJob> registrations = new Dictionary<JobRegistration, IJob>();
        private readonly ILogger<JobSchedulerSettings> logger;
        private readonly IJobFactory jobFactory;

        public JobSchedulerSettings(ILogger<JobSchedulerSettings> logger, IJobFactory jobFactory)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.jobFactory = jobFactory; // what to do when null?
        }

        public IDictionary<JobRegistration, IJob> Registrations => this.registrations;

        public JobSchedulerSettings Register(string cron, Action<string[]> action, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true) // TODO: not really needed
        {
            return this.Register(new JobRegistration(null, cron, null, isReentrant, timeout, enabled), new Job(action));
        }

        public JobSchedulerSettings Register(string key, string cron, Action<string[]> action, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            return this.Register(new JobRegistration(key, cron, null, isReentrant, timeout, enabled), new Job(action));
        }

        public JobSchedulerSettings Register(string cron, Func<string[], Task> task, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            return this.Register(new JobRegistration(null, cron, null, isReentrant, timeout, enabled), new Job(task));
        }

        public JobSchedulerSettings Register(string key, string cron, Func<string[], Task> task, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            return this.Register(new JobRegistration(key, cron, null, isReentrant, timeout, enabled), new Job(task));
        }

        public JobSchedulerSettings Register<T>(string cron, string[] args = null, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where T : IJob
        {
            return this.Register<T>(null, cron, args, isReentrant, timeout, enabled);
        }

        public JobSchedulerSettings Register<T>(string key, string cron, string[] args = null, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where T : IJob
        {
            if (!typeof(Job).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Job type to register must implement IJob.");
            }

            return this.Register(
                new JobRegistration(key, cron, args, isReentrant, timeout, enabled),
                new Job(async (t, a) => // defer job creation
                {
                    var job = this.jobFactory.CreateJob(typeof(T));
                    if (job == null)
                    {
                        throw new NaosException($"Cannot create job instance for type {typeof(T).PrettyName()}.");
                    }

                    await job.ExecuteAsync(t, a).ConfigureAwait(false);
                }));
        }

        public JobSchedulerSettings Register<T>(string key, string cron, Expression<Func<T, Task>> task, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            EnsureArg.IsNotNull(task, nameof(task));

            return this.Register(
                new JobRegistration(key, cron, null, isReentrant, timeout, enabled),
                new Job(async (t, a) => // defer job creation
                {
                    await Task.Run(() =>
                    {
                        var job = this.jobFactory.Create(typeof(T));
                        if (job == null)
                        {
                            throw new NaosException($"Cannot create job instance for type {typeof(T).PrettyName()}.");
                        }

                        var callExpression = task.Body as MethodCallExpression;
                        callExpression?.Method.Invoke(
                            job,
                            callExpression?.Arguments?.Select(p => this.MapParameter(p, t)).ToArray());
                    });
                }));
        }

        public JobSchedulerSettings Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));
            EnsureArg.IsNotNull(job, nameof(job));

            registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(job);
            this.logger.LogInformation($"register scheduled job (key={registration.Key}, cron={registration.Cron}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")}, enabled={registration.Enabled})");

            var item = this.registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(registration.Key));
            if (item.Key != null)
            {
                this.registrations.Remove(item.Key);
            }

            this.registrations.Add(registration, job);
            return this;
        }

        private object MapParameter(Expression expression, CancellationToken cancellationToken)
        {
            // https://gist.github.com/i-e-b/8556753
            if (expression.Type.IsValueType && expression.Type == typeof(CancellationToken))
            {
                return cancellationToken;
            }

            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }
    }
}