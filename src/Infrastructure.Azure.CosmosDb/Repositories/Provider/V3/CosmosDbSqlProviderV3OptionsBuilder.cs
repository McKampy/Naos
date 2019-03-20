﻿namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using Microsoft.Azure.Cosmos;
    using Naos.Core.Common;

    public class CosmosDbSqlProviderV3OptionsBuilder :
        BaseOptionsBuilder<CosmosDbSqlProviderV3Options, CosmosDbSqlProviderV3OptionsBuilder>
    {
        public CosmosDbSqlProviderV3OptionsBuilder Client(CosmosClient client)
        {
            this.Target.Client = client;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            this.Target.Client = new CosmosClient(connectionString);
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder Account(string endPoint, string key)
        {
            this.Target.AccountEndPoint = endPoint;
            this.Target.AccountKey = key;
            this.Target.Client = new CosmosClient(endPoint, key);
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder Database(string database)
        {
            this.Target.Database = database;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder Container(string container)
        {
            this.Target.Container = container;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder FilterScope(string partitionKeyPath)
        {
            this.Target.PartitionKeyPath = partitionKeyPath ?? "/Discriminator";
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder MessageScope(int throughPut)
        {
            if(throughPut < 400)
            {
                throughPut = 400;
            }

            this.Target.ThroughPut = throughPut;
            return this;
        }
    }
}