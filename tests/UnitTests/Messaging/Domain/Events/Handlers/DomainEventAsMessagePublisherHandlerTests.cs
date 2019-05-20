﻿namespace Naos.Core.UnitTests.Messaging.Domain.Events.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.UnitTests.Domain.Repositories;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    public class DomainEventAsMessagePublisherHandlerTests
    {
        [Fact]
        public async Task CanMap_Test()
        {
            // arrange
            var messageBroker = Substitute.For<IMessageBroker>();
            var domainEvent = new StubEntityDomainEvent(new StubEntity { FirstName = "John", LastName = "Doe", Id = "112233", Age = 25 });
            var sut = new DomainEventAsMessagePublisherHandler<StubEntityDomainEvent, StubMessage>(
                Substitute.For<ILogger<DomainEventAsMessagePublisherHandler<StubEntityDomainEvent, StubMessage>>>(),
                new StubMapper(),
                messageBroker);

            // act
            await sut.Handle(domainEvent, CancellationToken.None);

            // assert
            messageBroker.Received().Publish(Arg.Is<StubMessage>(m => m.FullName == "John Doe"));
        }

        public class StubEntityDomainEvent : DomainEvent
        {
            public StubEntityDomainEvent(StubEntity entity)
            {
                this.Entity = entity;
            }

            public StubEntity Entity { get; }
        }

        public class StubMessage : Message
        {
            public string FullName { get; set; }
        }

        public class StubMapper : IMapper<StubEntityDomainEvent, StubMessage>
        {
            public void Map(StubEntityDomainEvent source, StubMessage destination)
            {
                destination.FullName = $"{source.Entity?.FirstName} {source.Entity?.LastName}";
            }
        }
    }
}