﻿namespace Naos.Core.Operations.Domain
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class LogEvent : IEntity<string>, IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        public string Level { get; set; }

        public string Environment { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public string CorrelationId { get; set; }

        public string ServiceDescriptor { get; set; }

        public string SourceContext { get; set; }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public string GetAge()
        {
            var timestamp = this.Timestamp;
            if (timestamp.IsDefault())
            {
                return string.Empty;
            }

            return (DateTime.UtcNow - this.Timestamp).Humanize();
        }

        /// <summary>
        /// Determines whether this instance is transient (not persisted).
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is transient; otherwise, <c>false</c>.
        /// </returns>
        // public bool IsTransient() => this.Id.IsDefault();
    }
}
