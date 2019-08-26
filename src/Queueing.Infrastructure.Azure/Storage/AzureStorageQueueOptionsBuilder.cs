﻿namespace Naos.Core.Queueing.Infrastructure.Azure
{
    using System;
    using MediatR;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Naos.Foundation;

    public class AzureStorageQueueOptionsBuilder :
       BaseOptionsBuilder<AzureStorageQueueOptions, AzureStorageQueueOptionsBuilder>
    {
        public AzureStorageQueueOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Name(string name)
        {
            this.Target.Name = name;
            return this;
        }

        public AzureStorageQueueOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public AzureStorageQueueOptionsBuilder RetryPolicy(IRetryPolicy retryPolicy)
        {
            this.Target.RetryPolicy = retryPolicy;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Retries(int retries)
        {
            this.Target.Retries = retries;
            return this;
        }

        public AzureStorageQueueOptionsBuilder ProcessInterval(TimeSpan interval)
        {
            this.Target.ProcessInterval = interval;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public AzureStorageQueueOptionsBuilder DequeueInterval(TimeSpan interval)
        {
            this.Target.DequeueInterval = interval;
            return this;
        }
    }
}
