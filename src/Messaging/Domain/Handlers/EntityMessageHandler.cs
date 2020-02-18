﻿namespace Naos.Messaging.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class EntityMessageHandler<T> : IMessageHandler<EntityMessage<T>>
        where T : class, IEntity
    {
        public EntityMessageHandler(ILogger<EntityMessageHandler<T>> logger)
        {
            this.Logger = logger;
        }

        protected ILogger<EntityMessageHandler<T>> Logger { get; }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual Task Handle(EntityMessage<T> message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.Logger.BeginScope(loggerState))
            {
                this.Logger.LogInformation("{LogKey:l} handle (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Entity.GetType().Name, LogKeys.AppMessaging, message.GetType().PrettyName(), message.Id, message.Origin);
                Thread.Sleep(RandomGenerator.GenerateInt(500, 3500));
                return Task.CompletedTask;
            }
        }
    }
}