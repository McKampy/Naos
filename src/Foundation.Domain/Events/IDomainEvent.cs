﻿namespace Naos.Foundation.Domain
{
    using System;
    using MediatR;

    public interface IDomainEvent : INotification
    {
        /// <summary>
        /// The identifier of the event
        /// </summary>
        Guid EventId { get; set; }

        /// <summary>
        /// The correlation id of the event
        /// </summary>
        string CorrelationId { get; set; }
    }
}
