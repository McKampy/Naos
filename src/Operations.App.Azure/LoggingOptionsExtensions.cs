﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.IO;
    using EnsureThat;
    using global::Serilog;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure;
    using Naos.Core.Operations.App;

    public static class LoggingOptionsExtensions
    {
        public static LoggingOptions AddAzureDiagnosticsLogStream(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:azureDiagnosticsLogStream").Get<DiagnosticsLogStreamConfiguration>();
            if (configuration?.Enabled == true)
            {
                // configure the serilog sink
                // https://github.com/serilog/serilog-aspnetcore
                var path = configuration.File.EmptyToNull() ?? $"LogEvents_[PRODUCT]_[CAPABILITY]_[ENVIRONMENT].log"
                    .Replace("[ENVIRONMENT]", options.Environment)
                    .Replace("[PRODUCT]", options.Context.Descriptor?.Product)
                    .Replace("[CAPABILITY]", options.Context.Descriptor?.Capability);
                path = Path.Combine(@"D:\home\LogFiles", path);

                options.LoggerConfiguration?.WriteTo.File(
                    path,
                    //outputTemplate: diagnosticsLogStreamConfiguration.OutputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: configuration.FileSizeLimitBytes,
                    rollOnFileSizeLimit: configuration.RollOnFileSizeLimit,
                    rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), configuration.RollingInterval), // TODO: use tryparse
                    shared: true,
                    flushToDiskInterval: configuration.FlushToDiskIntervalSeconds.HasValue ? TimeSpan.FromSeconds(configuration.FlushToDiskIntervalSeconds.Value) : TimeSpan.FromSeconds(1));

                options.Context.Messages.Add($"{LogEventKeys.Startup} logging: azure diagnosticslogstream sink added (path={path})");
            }

            return options;
        }

        public static LoggingOptions AddAzureApplicationInsights(this LoggingOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:azureApplicationInsights").Get<ApplicationInsightsConfiguration>();
            if (configuration?.Enabled == true
                && configuration?.ApplicationKey.IsNullOrEmpty() == false)
            {
                // configure the serilog sink
                //options.LoggerConfiguration?.WriteTo.AppInsights(appInsightsConfiguration.ApplicationKey);

                options.Context.Messages.Add($"{LogEventKeys.Startup} logging: azure applicationinsightssink added (application={configuration.ApplicationKey})");
            }

            return options;
        }

        public static LoggingOptions UseAzureLogAnalytics(this LoggingOptions options, bool dashboardEnabled = true)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration?.GetSection("naos:operations:logging:azureLogAnalytics").Get<LogAnalyticsConfiguration>(); // TODO: move to operations:logevents:azureLogAnalytics
            if (configuration != null)
            {
                // configure the serilog sink
                var logName = configuration?.LogName.EmptyToNull() ?? "LogEvents_[ENVIRONMENT]"
                    .Replace("[ENVIRONMENT]", options.Environment)
                    .Replace("[PRODUCT]", options.Context.Descriptor?.Product)
                    .Replace("[CAPABILITY]", options.Context.Descriptor?.Capability);
                if (logName.IsNullOrEmpty())
                {
                    return options;
                }

                if (configuration.Enabled == true
                    && configuration.WorkspaceId.IsNullOrEmpty() == false
                    && configuration.AuthenticationId.IsNullOrEmpty() == false)
                {
                    options.LoggerConfiguration?.WriteTo.AzureAnalytics(
                        configuration.WorkspaceId,
                        configuration.AuthenticationId,
                        logName: logName, // without _CL
                        storeTimestampInUtc: true,
                        logBufferSize: configuration.BufferSize,
                        batchSize: configuration.BatchSize);

                    options.Context.Messages.Add($"{LogEventKeys.Startup} logging: azure loganalytics sink added (name={logName}_CL, workspace={configuration.WorkspaceId})");

                    if (dashboardEnabled) // registers the loganalytics repo which is used by the dashboard (NaosOperationsLogEventsController)
                    {
                        // configure the repository for the dashboard (controller)
                        options.Context.AddAzureLogAnalytics(logName);
                    }
                }
            }

            return options;
        }
    }
}
