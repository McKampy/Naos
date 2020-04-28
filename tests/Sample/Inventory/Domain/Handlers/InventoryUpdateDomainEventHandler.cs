﻿namespace Naos.Sample.Inventory.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class InventoryUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        protected InventoryUpdateDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<ProductInventory>() == true;
        }
    }
}