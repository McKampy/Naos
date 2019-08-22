﻿namespace Naos.Core.Commands.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RequestCommandDispatcherMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestCommandDispatcherMiddleware> logger;
        private readonly IMediator mediator;
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
            IMediator mediator,
            IOptions<RequestCommandDispatcherMiddlewareOptions> options)
        {
            EnsureArg.IsNotNull(next, nameof(next));
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.next = next;
            this.logger = logger;
            this.mediator = mediator;
            this.options = options.Value ?? new RequestCommandDispatcherMiddlewareOptions();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Equals(this.options.Registration.Route, StringComparison.OrdinalIgnoreCase)) // also match method
            {
                var commandRequest = SerializationHelper.JsonDeserialize("{\"FirstName\": \"John\",\"LastName\": \"Doe\"}", this.options.Registration.CommandType);
                var commandResponse = this.mediator.Send(commandRequest); // https://github.com/jbogard/MediatR/issues/385

                context.Response.StatusCode = this.options.Registration.ResponseStatusCodeOnSuccess;
                if (commandResponse?.Result != null)
                {
                    var jObject = JObject.FromObject(commandResponse.Result);
                    var result = jObject.SelectToken("result") ?? jObject.SelectToken("Result");

                    if (!result.IsNullOrEmpty())
                    {
                        await context.Response.WriteAsync(
                            SerializationHelper.JsonSerialize(result)).AnyContext();
                    }
                }

                // =terminating middlware
            }
            else
            {
                await this.next(context).AnyContext();
            }

            // TODO: map request body json to command typed as defined in options (.CommandType)  .... jsondeserialize<Type>
            //       send() typed command (mediator)
            //       command response.result > http response (json body)
        }
    }
}
