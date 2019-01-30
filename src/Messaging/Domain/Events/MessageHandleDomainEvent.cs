﻿namespace Naos.Core.Messaging.Domain
{
    using Naos.Core.Domain;
    using Naos.Core.Messaging.Domain.Model;

    public class MessageHandleDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandleDomainEvent"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageScope">The messageScope (servicename).</param>
        public MessageHandleDomainEvent(Message message, string messageScope)
        {
            this.Message = message;
            this.MessageScope = messageScope;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Message Message { get; set; }

        public string MessageScope { get; }
    }
}
