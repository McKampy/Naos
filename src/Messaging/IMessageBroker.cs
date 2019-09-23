﻿namespace Naos.Messaging
{
    using Naos.Messaging.Domain;

    /// <summary>
    /// Describes the interface of the messagebus.
    /// </summary>
    public interface IMessageBroker
    {
        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Publish(Message message);

        /// <summary>
        /// Subscribes for the message (TMessage) with a specific message handler (THandler).
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the message handler.</typeparam>
        IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>;

        /// <summary>
        /// Unsubscribes message (TMessage) and its message handler (THandler).
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the message handler.</typeparam>
        void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>;
    }
}