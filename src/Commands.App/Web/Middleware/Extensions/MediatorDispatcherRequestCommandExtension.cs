﻿namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Newtonsoft.Json.Linq;

    public class MediatorDispatcherRequestCommandExtension : RequestCommandExtension
    {
        private readonly ILogger<LoggingRequestCommandExtension> logger;
        private readonly IMediator mediator;

        public MediatorDispatcherRequestCommandExtension(
            ILogger<LoggingRequestCommandExtension> logger,
            IMediator mediator)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.logger = logger;
            this.mediator = mediator;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            RequestCommandRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command dispatch (name={registration.CommandType?.Name.SliceTill("Command").SliceTill("Query")}, id={command.Id}), type=mediator)", LogKeys.AppCommand);

            var response = await this.mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
            if (response != null)
            {
                var jObject = JObject.FromObject(response);

                if (!jObject.GetValueByPath<bool>("cancelled"))
                {
                    var resultToken = jObject.SelectToken("result") ?? jObject.SelectToken("Result");
                    registration?.OnSuccess?.Invoke(command, context);

                    if (!resultToken.IsNullOrEmpty())
                    {
                        context.Response.WriteJson(resultToken);
                    }
                }
                else
                {
                    var cancelledReason = jObject.GetValueByPath<string>("cancelledReason");
                    await context.Response.BadRequest(cancelledReason.SliceTill(":")).AnyContext();
                }
            }

            // the extension chain is terminated here
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command dispatch (name={registration.CommandType?.Name.SliceTill("Command").SliceTill("Query")}, id={command.Id}), type=mediator)", LogKeys.AppCommand);

            var response = await this.mediator.Send(command).AnyContext(); // https://github.com/jbogard/MediatR/issues/385
            if (response != null)
            {
                var jObject = JObject.FromObject(response);

                if (!jObject.GetValueByPath<bool>("cancelled"))
                {
                    registration?.OnSuccess?.Invoke(command, context);
                }
                else
                {
                    var cancelledReason = jObject.GetValueByPath<string>("cancelledReason");
                    await context.Response.BadRequest(cancelledReason.SliceTill(":")).AnyContext();
                }
            }

            // the extension chain is terminated here
        }
    }
}
