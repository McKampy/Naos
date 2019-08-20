﻿namespace Naos.Core.Commands.App.Web
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;

    public class RequestCommandDispatcherMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestCommandDispatcherMiddleware> logger;
        private readonly RequestCommandDispatcherMiddlewareOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCommandDispatcherMiddleware"/> class.
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public RequestCommandDispatcherMiddleware(
            RequestDelegate next,
            ILogger<RequestCommandDispatcherMiddleware> logger,
            IOptions<RequestCommandDispatcherMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.next = next;
            this.logger = logger;
            this.options = options.Value ?? new RequestCommandDispatcherMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context)
        {
            await this.next(context).AnyContext();

            // TODO: map request body json to command typed as defined in options (.Type)  .... jsondeserialize<Type>
            //       send typed command (mediator)
            //       command response.result > http response (json body)
        }
    }
}
