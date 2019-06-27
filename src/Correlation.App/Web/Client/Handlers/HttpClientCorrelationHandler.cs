﻿namespace Naos.Core.RequestCorrelation.App.Web
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class HttpClientCorrelationHandler : DelegatingHandler
    {
        private readonly ILogger<HttpClientCorrelationHandler> logger;
        private readonly ICorrelationContextAccessor correlationContext;

        public HttpClientCorrelationHandler(ILogger<HttpClientCorrelationHandler> logger, ICorrelationContextAccessor correlationContext)
        {
            this.logger = logger;
            this.correlationContext = correlationContext;
        }

        // TODO: add the current correlationid header to the outgoing CLIENT request header (get from ICorrelationContextAccessor)
        // TODO: generate a new unique request id and put in outgoing request headers
        // TODO: also add these headers to the RESPONSE message
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = this.correlationContext?.Context?.CorrelationId; // current correlationid will be set on outgoing request
            var requestId = RandomGenerator.GenerateString(5, false); // every outgoing request needs a unique id

            var loggerState = new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = correlationId
            };

            using(this.logger.BeginScope(loggerState))
            {
                this.logger.LogDebug($"{{LogKey:l}} [{requestId}] http added correlation headers", LogKeys.OutboundRequest);

                if(!correlationId.IsNullOrEmpty())
                {
                    request.Headers.Add("x-correlationid", correlationId);
                }

                request.Headers.Add("x-requestid", requestId);

                var response = await base.SendAsync(request, cancellationToken).AnyContext();

                if(!correlationId.IsNullOrEmpty())
                {
                    response.Headers.Add("x-correlationid", correlationId);
                }

                response.Headers.Add("x-requestid", requestId);

                return response;
            }
        }
    }
}
