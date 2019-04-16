﻿namespace Naos.Core.Common
{
    using System;
    using System.IO;

    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified object value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="output">The output.</param>
        void Serialize(object value, Stream output);

        /// <summary>
        /// Deserializes the specified input stream.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        object Deserialize(Stream input, Type type);

        /// <summary>
        /// Deserializes the specified input stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        T Deserialize<T>(Stream input);
    }
}
