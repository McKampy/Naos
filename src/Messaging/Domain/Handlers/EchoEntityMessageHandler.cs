﻿namespace Naos.Messaging.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class EchoEntityMessageHandler : EntityMessageHandler<EchoEntity>
    {
        public EchoEntityMessageHandler(ILogger<EchoEntityMessageHandler> logger)
            : base(logger)
        {
        }

        public override Task Handle(EntityMessage<EchoEntity> message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.Logger.BeginScope(loggerState))
            {
                this.Logger.LogInformation($"{{LogKey:l}} {message.Entity.Text} (name={{MessageName}}, id={{EventId}}, origin={{EventOrigin}})", LogKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}
