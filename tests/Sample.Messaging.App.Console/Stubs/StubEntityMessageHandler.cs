﻿namespace Naos.Core.Sample.Messaging.App.Console
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;

    public class StubEntityMessageHandler : EntityMessageHandler<StubEntity>
    {
        public StubEntityMessageHandler(ILogger<StubEntityMessageHandler> logger)
            : base(logger)
        {
        }

        public override Task Handle(EntityMessage<StubEntity> message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.logger.BeginScope(loggerState))
            {
                this.logger.LogInformation($"{LogEventIdentifiers.AppCommand} handle  (name={{MessageName}}, id={{EventId}}, origin={{EventOrigin}}) " + $"{message.Entity.FirstName} {message.Entity.LastName}", message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}
