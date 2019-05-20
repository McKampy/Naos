﻿namespace Naos.Core.Domain
{
    using MediatR;

    public interface IDomainEvent : INotification
    {
        string Id { get; }
    }
}
