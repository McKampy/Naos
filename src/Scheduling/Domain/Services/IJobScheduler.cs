﻿namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IJobScheduler
    {
        bool IsRunning { get; }

        IJobScheduler OnError(Action<Exception> errorHandler);

        IJobScheduler Register(string cron, Action<string[]> action); // TODO: not really needed

        IJobScheduler Register(string key, string cron, Action<string[]> action);

        IJobScheduler Register(string cron, Func<string[], Task> task);

        IJobScheduler Register(string key, string cron, Func<string[], Task> task);

        IJobScheduler Register(JobRegistration registration, IJob job);

        IJobScheduler Register<T>(string cron, string[] args = null)
            where T : IJob;

        IJobScheduler Register<T>(string key, string cron, string[] args = null)
            where T : IJob;

        IJobScheduler Register<T>(string key, string cron, Expression<Func<T, Task>> task);

        Task RunAsync();

        Task RunAsync(DateTime moment);

        Task TriggerAsync(string key, string[] args = null);

        Task TriggerAsync(string key, CancellationToken token, string[] args = null);

        IJobScheduler UnRegister(string key);

        IJobScheduler UnRegister(JobRegistration registration);
    }
}