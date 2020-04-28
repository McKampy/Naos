﻿namespace Naos.Foundation.Domain
{
    public class EntityUpdatedDomainEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdatedDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityUpdatedDomainEvent(IEntity entity)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IEntity Entity { get; set; }
    }
}
