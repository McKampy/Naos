﻿namespace Naos.Operations
{
    using Microsoft.Extensions.DependencyInjection;

    public class OperationsOptions
    {
        public OperationsOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}
