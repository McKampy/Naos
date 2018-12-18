﻿namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.App.Configuration;
    using Naos.Core.Operations.Domain.Repositories;
    using Shouldly;
    using Xunit;

    public class LogEventRepositoryTests : BaseTest
    {
        private readonly IServiceCollection services = new ServiceCollection();

        public LogEventRepositoryTests()
        {
            var configuration = NaosConfigurationFactory.CreateRoot();

            this.services
                .AddNaosOperationsSerilog(configuration)
                .AddNaosOperationsLogAnalytics(configuration)
                .AddNaosScheduling();

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }

        //[Fact]
        //public void VerifyContainer_Test()
        //{
        //    this.container.Verify();
        //}

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.ServiceProvider.GetService<ILogEventRepository>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public async Task FindAllAsync_Test()
        {
            var sut = this.ServiceProvider.GetService<ILogEventRepository>();
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }
    }
}
