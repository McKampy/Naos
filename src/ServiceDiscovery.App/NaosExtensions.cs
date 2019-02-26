﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web;
    //using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosOptions AddServiceDiscovery(
            this NaosOptions naosOptions,
            Action<ServiceDiscoveryOptions> setupAction = null,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.AddSingleton(sp =>
                naosOptions.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            naosOptions.Context.Services.AddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            setupAction?.Invoke(new ServiceDiscoveryOptions(naosOptions.Context));

            //context.Messages.Add($"{LogEventKeys.General} naos builder: service discovery added");

            return naosOptions;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions UseFileSystemClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new FileSystemServiceRegistry(
                    sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                    options.Context.Configuration?.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()));

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: service discovery added (type={nameof(FileSystemServiceRegistry)})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscovery", Description = "FileSystemClientRegistry" });

            return options;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions UseRouterClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            // client needs remote registry
            var registryConfiguration = options.Context.Configuration?.GetSection($"{section}:registry:router").Get<RouterServiceRegistryConfiguration>();
            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new RouterServiceRegistry(
                    sp.GetRequiredService<ILogger<RouterServiceRegistry>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    registryConfiguration));

            // overwrite ServiceDiscoveryConfiguration (from AddServiceDiscovery) with router infos
            options.Context.Services.AddSingleton(sp =>
                {
                    var configuration = options.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>();
                    configuration.RouterAddress = registryConfiguration.Address;
                    configuration.RouterPath = "api/servicediscovery/router";
                    configuration.RouterEnabled = true;
                    return configuration;
                });

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: service discovery added (type={nameof(RouterServiceRegistry)})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscovery", Description = "RouterClientRegistry", EchoUri = "api/echo/servicediscovery" });

            return options;
        }
    }
}
