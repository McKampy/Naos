﻿namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.ServiceContext.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the service context functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <returns></returns>
        public static NaosOptions AddServiceContext(
            this NaosOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            if (naosOptions.Context.Descriptor.Product.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a productName");
            }

            if (naosOptions.Context.Descriptor.Capability.IsNullOrEmpty())
            {
                throw new NaosException("SERVICE descriptor needs a capabilityName");
            }

            naosOptions.Context.Services.AddTransient<HttpClientServiceContextHandler>();
            naosOptions.Context.Services.AddSingleton(sp =>
                new Naos.Core.Common.ServiceDescriptor(
                    naosOptions.Context.Descriptor.Product,
                    naosOptions.Context.Descriptor.Capability,
                    naosOptions.Context.Descriptor.Version,
                    naosOptions.Context.Descriptor.Tags));

            naosOptions.Context.Messages.Add($"{LogEventKeys.General} naos builder: service context added");

            return naosOptions;
        }
    }
}
