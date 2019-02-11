﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Consul;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Consul;
    using Naos.Core.ServiceDiscovery.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions AddConsulClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton(sp => options.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(c =>
            {
                c.Address = new Uri(options.Context.Configuration?.GetSection($"{section}:registry:consul").Get<ConsulServiceRegistryConfiguration>().Address);
            }));
            options.Context.Services.TryAddSingleton<IServiceRegistry>(sp => new ConsulServiceRegistry(
                sp.GetRequiredService<ILogger<ConsulServiceRegistry>>(), sp.GetRequiredService<IConsulClient>()));
            options.Context.Services.TryAddSingleton<IServiceRegistryClient, ServiceRegistryClient>();

            return options;
        }
    }
}
