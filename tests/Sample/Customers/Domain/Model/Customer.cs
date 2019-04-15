﻿namespace Naos.Sample.Customers.Domain
{
    using System;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class Customer : Entity<string>, ITenantEntity, IAggregateRoot
    {
        public string CustomerNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string Region { get; set; }

        public Address Address { get; set; }

        public string TenantId { get; set; }

        public void SetCustomerNumber()
        {
            if(this.CustomerNumber.IsNullOrEmpty())
            {
                this.CustomerNumber = $"{RandomGenerator.GenerateString(2)}-{DateTime.UtcNow.Ticks}";
            }
        }
    }
}
