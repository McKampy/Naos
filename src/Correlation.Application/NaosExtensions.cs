﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Configuration.Application;
    using Naos.RequestCorrelation.Application;
    using Naos.RequestCorrelation.Application.Web;

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

            naosOptions.Context.Messages.Add("naos services builder: request correlation added");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "RequestCorrelation", EchoRoute = "naos/requestcorrelation/echo" });

            return naosOptions;
        }
    }
}
