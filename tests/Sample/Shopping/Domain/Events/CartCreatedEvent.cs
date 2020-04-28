﻿namespace Naos.Sample.Shopping.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class CartCreatedEvent : DomainEventBase<Guid>
    {
        internal CartCreatedEvent(Guid aggregateId, Guid customerId)
            : base(aggregateId)
        {
            this.CustomerId = customerId;
        }

        private CartCreatedEvent()
        {
        }

        private CartCreatedEvent(Guid aggregateId, long aggregateVersion, Guid customerId)
            : base(aggregateId, aggregateVersion)
        {
            this.CustomerId = customerId;
        }

        public Guid CustomerId { get; private set; }

        public override IDomainEvent<Guid> WithAggregate(Guid aggregateId, long aggregateVersion)
        {
            return new CartCreatedEvent(aggregateId, aggregateVersion, this.CustomerId);
        }
    }
}
