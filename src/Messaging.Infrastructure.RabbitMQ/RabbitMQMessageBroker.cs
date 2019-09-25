﻿namespace Naos.Messaging.Infrastructure.RabbitMQ
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Messaging.Domain;

    public class RabbitMQMessageBroker : IMessageBroker
    {
        private readonly RabbitMQMessageBrokerOptions options;
        private readonly ILogger<RabbitMQMessageBroker> logger;

        public RabbitMQMessageBroker(RabbitMQMessageBrokerOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Host, nameof(options.Host));
            EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));

            this.options = options;
            this.logger = options.CreateLogger<RabbitMQMessageBroker>();
        }

        public RabbitMQMessageBroker(Builder<RabbitMQMessageBrokerOptionsBuilder, RabbitMQMessageBrokerOptions> optionsBuilder)
            : this(optionsBuilder(new RabbitMQMessageBrokerOptionsBuilder()).Build())
        {
            // TODO: maybe use this client https://github.com/EasyNetQ/EasyNetQ  http://easynetq.com/
        }

        public void Publish(Message message)
        {
            throw new NotImplementedException();
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            return this;
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            throw new NotImplementedException();
        }
    }
}
