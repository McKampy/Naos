﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Tracing.Domain;
    using Naos.Tracing.Infrastructure;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static INaosBuilderContext AddMongoTracing(this INaosBuilderContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNull(context.Services, nameof(context.Services));

            var configuration = context.Configuration?.GetSection("naos:operations:logging:mongo").Get<MongoTracingConfiguration>();
            if (configuration != null)
            {
                context.Services.AddMongoClient("logging", new MongoConfiguration
                {
                    ConnectionString = configuration.ConnectionString?.Replace("[DATABASENAME]", configuration.DatabaseName),
                    DatabaseName = configuration.DatabaseName
                });

                context.Services.AddScoped<ILogTraceRepository>(sp =>
                {
                    return new MongoLogTraceRepository(o => o
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .MongoClient(sp.GetServices<IMongoClient>()
                            .FirstOrDefault(c => c.Settings.ApplicationName == "logging")) //TODO: make nice extension to get a named mongoclient
                        .Mapper(new AutoMapperEntityMapper(MongoMapperFactory.Create()))
                        .DatabaseName(configuration.DatabaseName)
                        .CollectionName(configuration.CollectionName));
                });
                context.Messages.Add($"naos services builder: logging azure mongo repository added (collection={configuration.CollectionName})");
            }

            return context;
        }
    }
}
