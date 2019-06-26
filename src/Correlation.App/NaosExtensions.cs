﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Configuration.App;
    using Naos.Core.RequestCorrelation.App;
    using Naos.Core.RequestCorrelation.App.Web;
    using Naos.Foundation;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the request correlation functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosServicesContextOptions AddRequestCorrelation(
            this NaosServicesContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            naosOptions.Context.Services.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
            naosOptions.Context.Services.AddTransient<HttpClientCorrelationHandler>();

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: request correlation added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "RequestCorrelation", EchoRoute = "api/echo/requestcorrelation" });

            return naosOptions;
        }
    }
}
