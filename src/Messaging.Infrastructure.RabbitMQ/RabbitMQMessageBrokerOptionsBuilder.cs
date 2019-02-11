﻿namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Core.Common;

    public class RabbitMQMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<RabbitMQMessageBrokerOptions, RabbitMQMessageBrokerOptionsBuilder>
    {
        public RabbitMQMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder HandlerFactory(IMessageHandlerFactory handlerFactory)
        {
            this.Target.HandlerFactory = handlerFactory;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Configuration(RabbitMQConfiguration configuration)
        {
            this.Target.Configuration = configuration;
            return this;
        }
    }
}