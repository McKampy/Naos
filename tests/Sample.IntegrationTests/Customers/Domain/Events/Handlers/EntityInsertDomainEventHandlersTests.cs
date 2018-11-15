﻿namespace Naos.Sample.IntegrationTests.Customers.Domain
{
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Core.Domain;
    using Naos.Sample.Customers.Domain;
    using Shouldly;
    using Xunit;

    public class EntityInsertDomainEventHandlersTests : BaseTest
    {
        [Fact]
        public async Task DomainEvent_Insert_Test()
        {
            // arrange
            var mediator = this.container.GetInstance<IMediator>();
            var entity = new Customer { FirstName = "FirstName1", LastName = "LastName1" };

            // act
            await mediator.Publish(new EntityInsertDomainEvent<IEntity>(entity)).ConfigureAwait(false);

            // assert
            entity.IdentifierHash.ShouldNotBeNull();
            entity.State.CreatedDate.ShouldNotBeNull();
            entity.State.UpdatedDate.ShouldNotBeNull();

            await mediator.Publish(new EntityInsertedDomainEvent<IEntity>(entity)).ConfigureAwait(false);
            // > CustomerInsertedDomainEventHandler
        }
    }
}
