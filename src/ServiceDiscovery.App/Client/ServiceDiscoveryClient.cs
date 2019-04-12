﻿namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.ServiceDiscovery.App.Web;

    public abstract class ServiceDiscoveryClient
    {
        protected ServiceDiscoveryClient(
            ILoggerFactory loggerFactory,
            ServiceDiscoveryConfiguration configuration,
            HttpClient httpClient,
            IServiceRegistryClient registryClient,
            string serviceName = null,
            string serviceTag = null)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(registryClient, nameof(registryClient));

            configuration = configuration ?? new ServiceDiscoveryConfiguration();
            this.Logger = loggerFactory.CreateLogger<ServiceDiscoveryClient>();

            if(configuration.RouterEnabled && !configuration.RouterAddress.IsNullOrEmpty())
            {
                // server-side (router)
                httpClient.BaseAddress = new Uri(new Uri(configuration.RouterAddress), configuration.RouterPath.Safe().TrimEnd('/') + "/"); // backslash mandatory https://stackoverflow.com/questions/23438416/why-is-httpclient-baseaddress-not-working

                // following are used by router to forward to correct service instance
                httpClient.DefaultRequestHeaders.Add(ServiceDiscoveryRouterHeaders.ServiceName, serviceName);
                httpClient.DefaultRequestHeaders.Add(ServiceDiscoveryRouterHeaders.ServiceTag, serviceTag);

                this.Logger.LogInformation($"{{LogKey:l}} router (service={{ServiceName}}, tag={serviceTag}, address={httpClient.BaseAddress})", LogKeys.ServiceDiscovery, serviceName);
            }
            else
            {
                // client-side
                if(serviceName.IsNullOrEmpty() && serviceTag.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("serviceName and serviceTag arguments cannot both be null or empty");
                }

                var registration = registryClient.RegistrationsAsync(serviceName, serviceTag).Result?.FirstOrDefault();
                if(registration != null)
                {
                    httpClient.BaseAddress = new Uri($"{registration.Address}:{registration.Port}".TrimEnd(':'));
                    this.Logger.LogInformation($"{{LogKey:l}} client (service={{ServiceName}}, tag={serviceTag}, address={httpClient.BaseAddress})", LogKeys.ServiceDiscovery, serviceName);
                }
                else
                {
                    this.Logger.LogWarning($"{{LogKey:l}} client (service={{ServiceName}}, tag={serviceTag}, address=not found in registry)", LogKeys.ServiceDiscovery, serviceName);
                }
            }

            // TODO: get serviceregistration by name OR any of the tags

            //else
            //{
            //    throw new Exception("no active service registrations found");
            //}

            this.HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public ILogger<ServiceDiscoveryClient> Logger { get; }
    }
}
