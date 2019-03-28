﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Common;

    public class NaosServicesContext : INaosServicesContext
    {
        public IServiceCollection Services { get; set; }

        public Naos.Core.Common.ServiceDescriptor Descriptor { get; set; }

        public string Environment { get; set; }

        public IConfiguration Configuration { get; set; }

        public List<string> Messages { get; set; } = new List<string>();

        public bool IsConsoleEnabled() => this.Configuration["console"] == "true";

        public INaosServicesContext AddTag(string tag)
        {
            this.Descriptor = this.Descriptor ?? new Naos.Core.Common.ServiceDescriptor();
            this.Descriptor.Tags = this.Descriptor.Tags.Insert(tag).ToArray();
            return this;
        }
    }
}
