﻿namespace Naos.Core.Discovery.App
{
    using System.Collections.Generic;
    using System.Linq;

    public class FileSystemServiceRegistry : IServiceRegistry
    {
        private readonly FileSystemServiceRegistryConfiguration configuration;

        public FileSystemServiceRegistry(FileSystemServiceRegistryConfiguration configuration)
        {
            this.configuration = configuration;

            // TODO: inject HealthStrategy which can validate the registrations
        }

        public void DeRegister(string id)
        {
            // TODO: remove registration
        }

        public void Register(ServiceRegistration registration)
        {
            // TODO: store registration
        }

        public IEnumerable<ServiceRegistration> Registrations()
        {
            // TODO: return list
            return Enumerable.Empty<ServiceRegistration>(); // omit unhealthy registrations!
        }
    }
}
