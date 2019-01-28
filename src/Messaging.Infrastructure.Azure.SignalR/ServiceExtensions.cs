﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure.Azure.SignalR;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosMessagingSignalR(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IMessageBroker> setupAction = null,
            string messageScope = null,
            string section = "naos:messaging:signalr")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            services.AddSingleton<IMessageBroker>(sp =>
            {
                var signalRConfiguration = configuration.GetSection(section).Get<SignalRConfiguration>();

                var result = new SignalRServerlessMessageBroker(
                        sp.GetRequiredService<ILogger<SignalRServerlessMessageBroker>>(),
                        (IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)),
                        new ServiceProviderMessageHandlerFactory(sp),
                        signalRConfiguration,
                        sp.GetRequiredService<IHttpClientFactory>(),
                        map: sp.GetRequiredService<ISubscriptionMap>(),
                        filterScope: Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty,
                        messageScope: messageScope); // PRODUCT.CAPABILITY;

                setupAction?.Invoke(result);
                return result;
            });

            return services;
        }
    }
}
