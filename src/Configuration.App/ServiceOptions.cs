﻿namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceOptions
    {
        public ServiceOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}