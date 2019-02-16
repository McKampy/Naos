﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using global::Serilog;
    using global::Serilog.Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Operations.App;

    public static class OperationsOptionsExtensions
    {
        private static IConfiguration internalConfiguration;
        private static LoggerConfiguration internalLoggerConfiguration;
        private static string internalEnvironment;
        private static string internalCorrelationId;
        private static ILoggerFactory factory;

        public static OperationsOptions AddLogging(
            this OperationsOptions options,
            Action<LoggingOptions> setupAction = null,
            string environment = null,
            string correlationId = null,
            LoggerConfiguration loggerConfiguration = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: logging added");
            internalConfiguration = options.Context?.Configuration;
            internalEnvironment = environment ?? Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production";
            internalCorrelationId = correlationId;
            internalLoggerConfiguration = internalLoggerConfiguration ?? loggerConfiguration;

            LoggingOptions loggingOptions = null;
            if (internalLoggerConfiguration == null)
            {
                internalLoggerConfiguration = new LoggerConfiguration();
                loggingOptions = new LoggingOptions(options.Context, internalLoggerConfiguration, internalEnvironment);
                InitializeLogger(internalLoggerConfiguration);

                setupAction?.Invoke(loggingOptions);
            }

            //options.Context.Services.AddSingleton(sp => CreateLoggerFactory());
            options.Context.Services.AddSingleton(sp =>
            {
                var factory = CreateLoggerFactory();
                //foreach (var message in loggingOptions?.Messages.Safe())
                //{
                //    Log.Logger.Debug(message);
                //}
                return factory;
            });
            options.Context.Services.AddSingleton(typeof(ILogger<>), typeof(LoggingAdapter<>));
            options.Context.Services.AddSingleton(typeof(Logging.ILogger), typeof(LoggingAdapter));

            return options;
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            if(factory == null) // extra singleton because sometimes this is called multiple times. serilog does not like that
            {
                Log.Logger = internalLoggerConfiguration.CreateLogger();
                factory = new LoggerFactory();
                factory.AddSerilog(Log.Logger);
                Log.Logger.Debug("{LogKey:l} logging: serilog initialized", LogEventKeys.Startup);
            }

            return factory;
        }

        private static void InitializeLogger(LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("HealthChecks.UI", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
#if DEBUG
                .WriteTo.Debug()
                .WriteTo.LiterateConsole(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    //outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {CorrelationId}|{Service}|{SourceContext}: {Message:lj}{NewLine}{Exception}");
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
#endif
                .Enrich.With(new ExceptionEnricher())
                .Enrich.With(new TicksEnricher())
                .Enrich.WithProperty(LogEventPropertyKeys.Environment, internalEnvironment)
                //.Enrich.WithProperty("ServiceDescriptor", internalServiceDescriptor)
                .Enrich.FromLogContext();

            if (!internalCorrelationId.IsNullOrEmpty())
            {
                loggerConfiguration.Enrich.WithProperty(LogEventPropertyKeys.CorrelationId, internalCorrelationId);
            }
        }
    }
}
