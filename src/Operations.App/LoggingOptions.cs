﻿namespace Naos.Operations.App
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class LoggingOptions
    {
        public LoggingOptions(
            INaosBuilderContext context,
            LoggerConfiguration loggerConfiguration)
        {
            this.Context = context;
            this.LoggerConfiguration = loggerConfiguration;
        }

        public INaosBuilderContext Context { get; }

        public LoggerConfiguration LoggerConfiguration { get; }
    }
}
