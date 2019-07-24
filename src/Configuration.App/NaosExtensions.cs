﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Configuration.App;
    using Naos.Foundation;
    using Console = Colorful.Console;

    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="product"></param>
        /// <param name="capability"></param>
        /// <param name="tags"></param>
        /// <param name="optionsAction"></param>
        /// <param name="environment"></param>
        /// <param name="section"></param>
        public static INaosServicesContext AddNaos(
            this IServiceCollection services,
            IConfiguration configuration,
            string product = null,
            string capability = null,
            string[] tags = null,
            Action<NaosServicesContextOptions> optionsAction = null,
            string environment = null,
            string section = "naos")
        {
            Console.WriteLine("--- naos service start", System.Drawing.Color.LimeGreen);
            EnsureArg.IsNotNull(services, nameof(services));

            var naosConfiguration = configuration?.GetSection(section).Get<NaosConfiguration>();
            var context = new NaosServicesContext
            {
                Services = services,
                Configuration = configuration,
                Environment = environment ?? Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production",
                Descriptor = new Naos.Foundation.ServiceDescriptor(
                    product ?? naosConfiguration.Product,
                    capability ?? naosConfiguration.Product,
                    tags: tags ?? naosConfiguration.Tags),
            };
            context.Messages.Add($"{LogKeys.Startup} naos services builder: naos services added");
            context.Services.AddSingleton(new NaosFeatureInformation { Name = "Naos", EchoRoute = "api/echo" });

            optionsAction?.Invoke(new NaosServicesContextOptions(context));

            try
            {
                var logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("Naos");
                foreach(var message in context.Messages.Safe())
                {
                    logger.LogInformation(message);
                }
            }
            catch(InvalidOperationException)
            {
                // no loggerfactory registered
                services.AddScoped<ILoggerFactory>(sp => new LoggerFactory());
            }

            return context;
        }

        public static NaosServicesContextOptions AddServices(
            this NaosServicesContextOptions naosOptions,
            Action<ServiceOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            optionsAction?.Invoke(new ServiceOptions(naosOptions.Context));

            return naosOptions;
        }
    }
}
