﻿namespace Naos.Messaging.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class TrackMessageDomainEventHandler
        : IDomainEventHandler<MessagePublishedDomainEvent>, IDomainEventHandler<MessageHandledDomainEvent>
    {
        private readonly ILogger<TrackMessageDomainEventHandler> logger;

        public TrackMessageDomainEventHandler(ILogger<TrackMessageDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(MessagePublishedDomainEvent notification)
        {
            return true;
        }

        public bool CanHandle(MessageHandledDomainEvent notification)
        {
            return true;
        }

        public async Task Handle(MessagePublishedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (notification == null)
                {
                    return;
                }

                this.logger.LogJournal(LogKeys.Messaging, $"[{notification.Message?.Identifier}] message published: {notification.Message?.GetType().PrettyName()} (id={notification.Message?.Id}, origin={notification.Message?.Origin})", LogPropertyKeys.TrackPublishMessage);
            }).AnyContext();
        }

        public async Task Handle(MessageHandledDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (notification == null)
                {
                    return;
                }

                this.logger.LogJournal(LogKeys.Messaging, $"[{notification.Message?.Identifier}] message handled : {notification.Message?.GetType().PrettyName()} (id={notification.Message?.Id}, service={notification.MessageScope}, origin={notification.Message?.Origin})", LogPropertyKeys.TrackReceiveMessage);
            }).AnyContext();
        }
    }
}