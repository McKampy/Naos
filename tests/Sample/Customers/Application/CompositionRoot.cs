﻿namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure.Azure;
    using Naos.Sample.Customers.Application.Client;
    using Naos.Sample.Customers.Domain;
    using Naos.Sample.Customers.Infrastructure;
    using Naos.Tracing.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions AddCustomersModule(
            this ModuleOptions options,
            string section = "naos:sample:customers")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("customers");
            options.Context.AddServiceClient<UserAccountsClient>();

            var configuration = options.Context.Configuration?.GetSection($"{section}:cosmosDb").Get<CosmosConfiguration>() ?? new CosmosConfiguration();
            options.Context.Services.AddScoped<ICustomerRepository>(sp =>
            {
                return new CustomerRepository(
                    new RepositoryTracingDecorator<Customer>(
                        sp.GetService<ILogger<CustomerRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<Customer>(
                            sp.GetRequiredService<ILogger<CustomerRepository>>(),
                            new RepositoryTenantDecorator<Customer>(
                                "naos_sample_test",
                                new CosmosSqlRepository<Customer>(o => o
                                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                    .Mediator(sp.GetRequiredService<IMediator>())
                                    .Provider(sp.GetRequiredService<ICosmosSqlProvider<Customer>>())))))); // v3
                                    //.Provider(new CosmosDbSqlProviderV2<Customer>( // v2
                                    //    logger: sp.GetRequiredService<ILogger<CosmosDbSqlProviderV2<Customer>>>(),
                                    //    client: CosmosDbClientV2.Create(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken),
                                    //    databaseId: cosmosDbConfiguration.DatabaseId,
                                    //    collectionIdFactory: () => cosmosDbConfiguration.CollectionId,
                                    //    partitionKeyPath: cosmosDbConfiguration.CollectionPartitionKey,
                                    //    throughput: cosmosDbConfiguration.CollectionOfferThroughput,
                                    //    isMasterCollection: cosmosDbConfiguration.IsMasterCollection)))))));
            }).AddScoped<ICosmosSqlProvider<Customer>>(sp =>
            {
                return new CosmosSqlProviderV3<Customer>(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Account(configuration.ServiceEndpointUri, configuration.AuthKeyOrResourceToken)
                    .Database(configuration.DatabaseId)
                    .PartitionKey(e => e.Region));
            });

            options.Context.Services.AddScoped<IOrderRepository>(sp =>
            {
                return new OrderRepository(
                    new RepositoryTracingDecorator<Order>(
                        sp.GetService<ILogger<OrderRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<Order>(
                            sp.GetRequiredService<ILogger<OrderRepository>>(),
                            new RepositoryTenantDecorator<Order>(
                                "naos_sample_test",
                                new CosmosSqlRepository<Order, DtoOrder>(o => o
                                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                    .Mediator(sp.GetRequiredService<IMediator>())
                                    .Provider(sp.GetRequiredService<ICosmosSqlProvider<DtoOrder>>())
                                    .Mapper(new AutoMapperEntityMapper(MapperFactory.Create()))))))); // v3
            }).AddScoped<ICosmosSqlProvider<DtoOrder>>(sp =>
            {
                return new CosmosSqlProviderV3<DtoOrder>(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Account(configuration.ServiceEndpointUri, configuration.AuthKeyOrResourceToken)
                    .Database(configuration.DatabaseId)
                    .Container("orders")
                    .PartitionKey(e => e.Location));
            });

            var queueStorageConfiguration = options.Context.Configuration?.GetSection($"{section}:queueStorage").Get<QueueStorageConfiguration>();
            options.Context.Services.AddSingleton<IQueue<Customer>>(sp =>
            {
                var queue = new AzureStorageQueue<Customer>(
                    new AzureStorageQueueOptionsBuilder()
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .ConnectionString(queueStorageConfiguration.ConnectionString).Build());

                // for testing enqueue some items
                _ = queue.EnqueueAsync(new Customer()).Result;
                _ = queue.EnqueueAsync(new Customer()).Result;
                return queue;
            });

            options.Context.Services
                .AddHealthChecks()
                    .AddAzureQueueStorage(
                        queueStorageConfiguration.ConnectionString,
                        name: "Customers-queueStorage");

            //options.Context.Services.AddSingleton<IValidator<CreateCustomerCommand>>(new CreateCustomerCommandValidator());

            options.Context.Services
                .AddHealthChecks()
                    .AddDocumentDb(s =>
                        {
                            s.UriEndpoint = configuration.ServiceEndpointUri;
                            s.PrimaryKey = configuration.AuthKeyOrResourceToken;
                        },
                        name: "Customers-cosmosdb")
                    .AddServiceDiscoveryClient<UserAccountsClient>();

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: customers service added");

            return options;
        }
    }
}
