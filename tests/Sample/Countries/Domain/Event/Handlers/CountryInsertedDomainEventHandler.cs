﻿namespace Naos.Sample.Countries.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CountryInsertedDomainEventHandler
        : DomainEventHandlerBase<EntityInsertedDomainEvent>
    {
        public CountryInsertedDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityInsertedDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }

        public override async Task Process(EntityInsertedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                this.Logger.LogInformation($"{{LogKey:l}} process {notification.GetType().Name.SliceTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogKeys.DomainEvent);
                // TODO: do something, trigger message (integration)
            }).AnyContext();
        }
    }
}
