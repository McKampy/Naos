﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Naos.Core.Operations.App;
    using Naos.Core.Operations.App.Web;

    public static class OperationsOptionsExtensions
    {
        public static OperationsOptions AddRequestStorage(
            this OperationsOptions options,
            Action<RequestStorageOptions> setupAction = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            setupAction?.Invoke(new RequestStorageOptions(options.Context));

            return options;
        }
    }
}
