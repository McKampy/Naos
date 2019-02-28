﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;

    public class CommandsOptions
    {
        public CommandsOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}
