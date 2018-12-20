﻿namespace Naos.Core.Common.Web
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class HttpClientLogHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        // https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
        private readonly ILoggerFactory loggerFactory;

        public HttpClientLogHandlerBuilderFilter(ILoggerFactory loggerFactory)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.loggerFactory = loggerFactory;
        }

        public Action<Microsoft.Extensions.Http.HttpMessageHandlerBuilder> Configure(Action<Microsoft.Extensions.Http.HttpMessageHandlerBuilder> next)
        {
            EnsureArg.IsNotNull(next, nameof(next));

            return (builder) =>
            {
                next(builder);
                builder.AdditionalHandlers.Insert(0, new HttpClientLogHandler(
                    this.loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler")));
            };
        }
    }
}
