﻿namespace Naos.Core.Messaging.Domain
{
    using System.Collections.Generic;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class Message
        : IEntity<string>, IHaveDiscriminator, IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
            this.Id = IdGenerator.Instance.Next;
            this.Identifier = RandomGenerator.GenerateString(5, false);
        }

        /// <summary>
        /// Gets or sets the identifier of this message.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        [JsonIgnore] // needed for FolderBasedBroker::Publish (JsonSerialize ID issue)
        public string Id { get; set; }

        [JsonProperty(PropertyName = "id")]
        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        /// <summary>
        /// Gets the type of the entity (discriminator).
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public string Discriminator => this.GetType().FullName;

        /// <summary>
        /// Gets or sets the short identifier for this message.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the origin service name of this <see cref="Message"/> instance.
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        public string Origin { get; set; } // Product.Capability

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public MessageStatus Status { get; set; }

        public DomainEvents DomainEvents => new DomainEvents();
    }
}
