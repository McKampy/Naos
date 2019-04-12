﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using global::Serilog;
    using global::Serilog.Events;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;
    using Naos.Core.Common.Console.App;
    using Naos.Core.Common.Web;
    using Naos.Core.Operations.App;

    [ExcludeFromCodeCoverage]
    public static class OperationsOptionsExtensions
    {
        private static string internalCorrelationId;
        private static ILoggerFactory factory;

        public static OperationsOptions AddLogging(
        this OperationsOptions options,
        Action<LoggingOptions> optionsAction = null,
        string environment = null,
        string correlationId = null,
        LoggerConfiguration loggerConfiguration = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: logging added");
            internalCorrelationId = correlationId;

            var loggingOptions = new LoggingOptions(
                options.Context,
                loggerConfiguration ?? new LoggerConfiguration());

            InitializeLogger(loggingOptions);
            optionsAction?.Invoke(loggingOptions);

            options.Context.Services.AddSingleton(sp => CreateLoggerFactory(loggingOptions));
            options.Context.Services.AddSingleton(typeof(ILogger<>), typeof(LoggingAdapter<>));
            options.Context.Services.AddSingleton(typeof(Logging.ILogger), typeof(LoggingAdapter));

            options.Context.Services.AddTransient<HttpClientLogHandler>();
            options.Context.Services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());

            return options;
        }

        public static OperationsOptions AddInteractiveConsole(
            this OperationsOptions options,
            bool enabled = true)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            if(options.Context.IsConsoleEnabled() && enabled)
            {
                // needed for mediator, register console commands + handlers
                options.Context.Services.Scan(scan => scan
                    .FromApplicationDependencies()
                    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("ConsoleCommand") || c.Name.EndsWith("ConsoleCommandEventHandler")))
                    .AsImplementedInterfaces());

                options.Context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    {
                        return new InteractiveConsoleHostedService(
                          sp.GetRequiredService<ILoggerFactory>(),
                          (IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)),
                          sp.GetServices<IConsoleCommand>());
                    });
            }

            return options;
        }

        private static ILoggerFactory CreateLoggerFactory(LoggingOptions loggingOptions)
        {
            if(factory == null) // extra singleton because sometimes this is called multiple times. serilog does not like that
            {
                Log.Logger = loggingOptions.LoggerConfiguration.CreateLogger();

                factory = new LoggerFactory();
                factory.AddSerilog(Log.Logger);
                Log.Logger.Debug("{LogKey:l} naos services builder: logging initialized (type=Serilog)", LogKeys.Startup);
            }

            return factory;
        }

        private static void InitializeLogger(LoggingOptions loggingOptions)
        {
            loggingOptions.LoggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("HealthChecks.UI", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
#if DEBUG
                .WriteTo.Debug()
#endif
                //.Enrich.With<EventTypeEnricher>() // https://nblumhardt.com/2015/10/assigning-event-types-to-serilog-events/
                .Enrich.With(new IdEnricher())
                .Enrich.With(new ExceptionEnricher())
                .Enrich.With(new TicksEnricher())
                .Enrich.WithProperty(LogEventPropertyKeys.Environment, loggingOptions.Context.Environment)
                .Enrich.WithProperty(LogEventPropertyKeys.ServiceName, loggingOptions.Context.Descriptor.Name)
                .Enrich.WithProperty(LogEventPropertyKeys.ServiceProduct, loggingOptions.Context.Descriptor.Product)
                .Enrich.WithProperty(LogEventPropertyKeys.ServiceCapability, loggingOptions.Context.Descriptor.Capability)
                .Enrich.FromLogContext();

            if(!internalCorrelationId.IsNullOrEmpty())
            {
                loggingOptions.LoggerConfiguration.Enrich.WithProperty(LogEventPropertyKeys.CorrelationId, internalCorrelationId);
            }
        }
    }
}
