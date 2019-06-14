﻿namespace Naos.Foundation.Infrastructure
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Microsoft.Azure.ServiceBus;

    public interface IServiceBusProvider // TODO: move to infra.sb
    {
        ServiceBusConnectionStringBuilder ConnectionStringBuilder { get; }

        string EntityPath { get; }

        ITopicClient CreateModel();

        Task<ITopic> EnsureTopic(string topicName);

        Task<ISubscription> EnsureSubscription(string topicName, string subscriptionName);
    }
}
