﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure.FileSystem;

    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddMessagingFileSystem(
            this ServiceConfigurationContext context,
            Action<IMessageBroker> setupAction = null,
            string messageScope = null,
            string section = "naos:messaging:fileSystem")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var result = new FileSystemMessageBroker(
                        sp.GetRequiredService<ILogger<FileSystemMessageBroker>>(),
                        (IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)),
                        new ServiceProviderMessageHandlerFactory(sp),
                        context.Configuration.GetSection(section).Get<FileSystemConfiguration>(),
                        map: sp.GetRequiredService<ISubscriptionMap>(),
                        filterScope: Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty,
                        messageScope: messageScope); // PRODUCT.CAPABILITY;

                setupAction?.Invoke(result);
                return result;
            });

            return context;
        }
    }
}
