﻿namespace Naos.Sample.Inventory.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Sample.Inventory.Domain;

    public class SeederStartupTask : IStartupTask
    {
        private readonly ILogger<SeederStartupTask> logger;
        private readonly IInventoryRepository repository;

        public SeederStartupTask(ILoggerFactory loggerFactory, IInventoryRepository repository)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = loggerFactory.CreateLogger<SeederStartupTask>();
            this.repository = repository;
        }

        public TimeSpan? Delay { get; set; } = new TimeSpan(0, 0, 5); //TimeSpan.Zero;

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            new List<ProductInventory>
            {
                new ProductInventory
                {
                    Id = "548fb10e-2ad4-4bd1-9b33-6414a5ce7b28",
                    Number = "AA1234",
                    Quantity = 99,
                    Region = "East"
                },
                new ProductInventory
                {
                    Id = "558fb10e-2ad4-4bd1-9b33-6414a5ce7b28",
                    Number = "AA1234",
                    Quantity = 88,
                    Region = "West"
                },
                new ProductInventory
                {
                    Id = "558fb10f-2ad4-4bd1-9b33-6414a5ce7b28",
                    Number = "BB1234",
                    Quantity = 77,
                    Region = "East"
                }
            }.ForEach(async e =>
            {
                if (!await this.repository.ExistsAsync(e.Id).AnyContext())
                {
                    await this.repository.InsertAsync(e).AnyContext();
                }
            });

            return Task.CompletedTask;
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
