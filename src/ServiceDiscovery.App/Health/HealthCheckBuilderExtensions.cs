﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using HealthChecks.Uris;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Naos.Core.Common;
    using Naos.Core.ServiceDiscovery.App;

    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddServiceDiscoveryProxy<T>(this IHealthChecksBuilder builder, string name = null, string route = "echo", HealthStatus? failureStatus = null, IEnumerable<string> tags = null)
            where T : ServiceDiscoveryProxy
        {
            name = name ?? typeof(T).Name;
            if (name.EndsWith("Proxy", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Replace("Proxy", "-proxy", StringComparison.OrdinalIgnoreCase);
            }

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => CreateHealthCheck<T>(sp, name, route),
                failureStatus,
                tags));
        }

        private static UriHealthCheck CreateHealthCheck<T>(IServiceProvider sp, string name, string route)
            where T : ServiceDiscoveryProxy
        {
            var proxy = sp.GetRequiredService<T>();
            if(proxy == null)
            {
                throw new NaosException($"Health: ServiceDiscovery proxy '{typeof(T)}' not found, please add with service.AddHttpClient<ServiceDiscoveryProxy>()");
            }

            var options = new UriHealthCheckOptions();
            var address = proxy.HttpClient?.BaseAddress?.ToString();
            if (address.IsNullOrEmpty())
            {
                throw new NaosException($"Health: ServiceDiscovery proxy '{typeof(T)}' address not found, registration inactive (due to health) or missing from registry?");
            }

            options.AddUri(new Uri($"{address.TrimEnd('/')}/{route}"));

            //var httpClientFactory = sp.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            return new UriHealthCheck(options, () => proxy.HttpClient); // httpClientFactory.CreateClient(name)
        }
    }
}
