﻿namespace Naos.Messaging.Infrastructure
{
    using System;
    using global::RabbitMQ.Client;

    public interface IRabbitMQProvider
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
