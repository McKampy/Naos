﻿namespace Naos.Sample.Customers.Domain
{
    using System;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Newtonsoft.Json;

    public class SimpleCustomer : IDiscriminated
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string CustomerNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string Region { get; set; }

        public Address Address { get; set; }

        public string TenantId { get; set; }

        public string Discriminator => this.GetType().FullPrettyName();

        public void SetCustomerNumber()
        {
            if (this.CustomerNumber.IsNullOrEmpty())
            {
                this.CustomerNumber = $"{RandomGenerator.GenerateString(2)}-{DateTime.UtcNow.Ticks}";
            }
        }
    }
}
