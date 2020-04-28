﻿namespace Naos.Sample.UserAccounts.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class UserAccountUpdatedDomainEventHandler
        : DomainEventHandlerBase<EntityUpdatedDomainEvent>
    {
        public UserAccountUpdatedDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityUpdatedDomainEvent notification)
        {
            return notification?.Entity.Is<UserAccount>() == true;
        }

        public override async Task Process(EntityUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                this.Logger.LogInformation($"{{LogKey:l}} handle {notification.GetType().Name.SliceTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogKeys.DomainEvent);
                // TODO: do something, trigger message (integration)
            }).AnyContext();
        }
    }
}