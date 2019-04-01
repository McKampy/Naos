﻿namespace Naos.Core.Common.Serialization
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    public class BsonDataSerializer : ITextSerializer
    {
        private readonly JsonSerializer serializer;

        public BsonDataSerializer(JsonSerializerSettings settings = null)
        {
            this.serializer = JsonSerializer.Create(settings ?? DefaultJsonSerializerSettings.Create());
        }

        public void Serialize(object value, Stream output)
        {
            using (var writer = new BsonDataWriter(output))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, value);
                //writer.Flush();
            }
        }

        public object Deserialize(Stream input, Type type)
        {
            using (var reader = new BsonDataReader(input))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(reader, type);
            }
        }

        public T Deserialize<T>(Stream input)
        {
            using (var reader = new BsonDataReader(input))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
