﻿namespace Naos.Core.Domain
{
    using Newtonsoft.Json;

    /// <summary>
    /// A base entity for all tenant related entities
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <seealso cref="Naos.Core.Domain.Entity{TId}" />
    public abstract class TenantEntity<TId> : Entity<TId>
    {
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        public override string ToString() => $"{this.EntityType} [Id={this.Id}, Tentant={this.TenantId}]";
    }
}
