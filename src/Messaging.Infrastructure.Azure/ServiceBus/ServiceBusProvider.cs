﻿namespace Naos.Core.Messaging.Infrastructure.Azure.ServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Infrastructure.ServiceBus;

    public class ServiceBusProvider : IServiceBusProvider // TODO: move to infra.sb
    {
        private readonly ILogger<ServiceBusProvider> logger;
        private readonly IServiceBusNamespace serviceBusNamespace;
        private ITopicClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusProvider" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="configuration">The configuration.</param>
        public ServiceBusProvider(
            ILogger<ServiceBusProvider> logger,
            AzureCredentials credentials,
            IOptions<ServiceBusConfiguration> configuration)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(credentials, nameof(credentials));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNull(configuration.Value, nameof(configuration.Value));
            EnsureArg.IsNotNullOrEmpty(configuration.Value.SubscriptionId, nameof(configuration.Value.SubscriptionId));
            EnsureArg.IsNotNullOrEmpty(configuration.Value.ConnectionString, nameof(configuration.Value.ConnectionString));
            EnsureArg.IsNotNullOrEmpty(configuration.Value.ResourceGroup, nameof(configuration.Value.ResourceGroup));
            EnsureArg.IsNotNullOrEmpty(configuration.Value.NamespaceName, nameof(configuration.Value.NamespaceName));
            EnsureArg.IsNotNullOrEmpty(configuration.Value.EntityPath, nameof(configuration.Value.EntityPath));

            this.logger = logger;
            this.ConnectionStringBuilder = new ServiceBusConnectionStringBuilder(configuration.Value.ConnectionString)
            {
                EntityPath = configuration.Value.EntityPath
            };
            var serviceBusManager = ServiceBusManager.Authenticate(credentials, configuration.Value.SubscriptionId);
            this.serviceBusNamespace = serviceBusManager.Namespaces.GetByResourceGroup(configuration.Value.ResourceGroup, configuration.Value.NamespaceName);
        }

        /// <summary>
        /// Gets the connection string builder.
        /// </summary>
        /// <value>
        /// The connection string builder.
        /// </value>
        public ServiceBusConnectionStringBuilder ConnectionStringBuilder { get; }

        public ITopicClient CreateModel()
        {
            if(this.client == null)
            {
                this.client = new TopicClient(this.ConnectionStringBuilder, RetryPolicy.Default);
            }

            if (this.client.IsClosedOrClosing)
            {
                this.logger.LogInformation("create new servicebus topic client instance");
                this.client = new TopicClient(this.ConnectionStringBuilder, RetryPolicy.Default);
            }

            return this.client;
        }

        /// <summary>
        /// Ensures the topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns></returns>
        public async Task<ITopic> EnsureTopic(string topicName)
        {
            var topics = await this.serviceBusNamespace.Topics.ListAsync();
            var topic = topics.FirstOrDefault(t => t.Name == topicName);

            if (topic == null)
            {
                this.logger.LogDebug($"create servicebus topic: {topicName}");
                topic = await this.serviceBusNamespace.Topics.Define(topicName).CreateAsync();
            }
            else
            {
                this.logger.LogDebug($"found servicebus topic: {topicName}");
            }

            return topic;
        }

        /// <summary>
        /// Ensures the topic and subscription.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns></returns>
        public async Task<ISubscription> EnsureSubscription(string topicName, string subscriptionName)
        {
            var topic = await this.EnsureTopic(topicName);

            var subscriptions = await topic.Subscriptions.ListAsync();
            var subscription = subscriptions.FirstOrDefault(s => s.Name == subscriptionName);

            if (subscription == null)
            {
                this.logger.LogDebug($"create servicebus topic/subscription: {topicName}/{subscriptionName}");
                await topic.Subscriptions.Define(subscriptionName).CreateAsync();
            }
            else
            {
                this.logger.LogDebug($"found servicebus topic/subscription: {topicName}/{subscriptionName} (messageCount={subscription.MessageCount})");
            }

            return subscription;
        }
    }
}
