﻿namespace Naos.Core.Messaging.Domain
{
    using System;

    public interface IMessageHandlerFactory
    {
        /// <summary>
        /// Creates the specified message handler type.
        /// </summary>
        /// <param name="messageHandlerType">Type of the message handler.</param>
        /// <returns></returns>
        object Create(Type messageHandlerType);
    }
}