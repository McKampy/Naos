﻿namespace Microsoft.AspNetCore.Builder
{
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.App.Web;
    using Naos.Foundation;

    /// <summary>
    /// Extension methods for the request command middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseRequestCommands(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

#pragma warning disable IDE0067 // Dispose objects before losing scope
#pragma warning disable CA2000 // Dispose objects before losing scope
            //var registrations = naosOptions.Context.Application.ApplicationServices.CreateScope().ServiceProvider.GetServices<RequestCommandRegistration>();
            var registrations = naosOptions.Context.Application.ApplicationServices.GetServices<RequestCommandRegistration>();

            foreach (var registration in registrations.Safe().Where(r => !r.Route.IsNullOrEmpty()))
            {
                naosOptions.Context.Application.UseMiddleware<RequestCommandDispatcherMiddleware>(
                    Options.Create(new RequestCommandDispatcherMiddlewareOptions
                    {
                        CommandType = registration.CommandType,
                        OnSuccessStatusCode = registration.OnSuccessStatusCode,
                        OpenApiDescription = registration.OpenApiDescription,
                        OpenApiGroupName = registration.OpenApiGroupName,
                        OpenApiGroupPrefix = registration.OpenApiGroupPrefix,
                        OpenApiProduces = registration.OpenApiProduces,
                        OpenApiResponseDescription = registration.OpenApiResponseDescription,
                        OpenApiSummary = registration.OpenApiSummary,
                        RequestMethod = registration.RequestMethod,
                        ResponseType = registration.ResponseType,
                        Route = registration.Route
                    }));
                naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: request command added (route={registration.Route}, type={registration.CommandType.PrettyName()})");
            }

            return naosOptions;
        }
    }
}
