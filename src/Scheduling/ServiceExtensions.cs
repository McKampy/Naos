﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling;
    using Naos.Core.JobScheduling.App.Web;
    using Naos.Core.JobScheduling.Domain;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddJobScheduling(
        this ServiceConfigurationContext context,
        Action<JobSchedulerSettings> setupAction = null,
        string section = "naos:scheduling")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            //container.RegisterSingleton<IHostedService>(() =>
            //{
            //    return new SchedulerHostedService(
            //        container.GetInstance<ILogger<SchedulerHostedService>>(),
            //        container);
            //}/*, Lifestyle.Scoped*/);

            // TODO: temporary solution to get the scheduler hosted service to run (with its dependencies)
            // https://stackoverflow.com/questions/50394666/injecting-simple-injector-components-into-ihostedservice-with-asp-net-core-2-0#
            context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new JobSchedulerHostedService(sp.GetRequiredService<ILogger<JobSchedulerHostedService>>(), sp));

            //container.RegisterSingleton<IJobScheduler>(() =>
            //{
            //    var settings = new JobSchedulerSettings(
            //        container.GetInstance<ILogger<JobSchedulerSettings>>(),
            //        new SimpleInjectorJobFactory(container));
            //    setupAction?.Invoke(settings);

            //    var result = new JobScheduler(
            //        container.GetInstance<ILogger<JobScheduler>>(),
            //        new InProcessMutex(container.GetInstance<ILogger<InProcessMutex>>()),
            //        settings);

            //    return result;
            //});

            context.Services.AddSingleton<IJobScheduler>(sp =>
            {
                var settings = new JobSchedulerSettings(
                    sp.GetRequiredService<ILogger<JobSchedulerSettings>>(),
                    new ServiceProviderJobFactory(sp));
                setupAction?.Invoke(settings);

                return new JobScheduler(
                    sp.GetRequiredService<ILogger<JobScheduler>>(),
                    new InProcessMutex(sp.GetRequiredService<ILogger<InProcessMutex>>()),
                    settings);
            });

            return context;
        }
    }
}
