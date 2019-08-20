﻿namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.App.Web;
    using Naos.Core.Commands.Domain;
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

            var registrations = naosOptions.Context.Application.ApplicationServices.GetServices<RequestCommandRegistration>();

            foreach (var registration in registrations.Safe())
            {
                naosOptions.Context.Application.UseMiddleware<RequestCommandDispatcherMiddleware>(
                    Options.Create(new RequestCommandDispatcherMiddlewareOptions
                    {
                        Route = registration.Route,
                        CommandType = registration.Type,
                        RequestMethod = registration.RequestMethod
                    }));
                naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: request command added (route={registration.Route}, type={registration.Type.PrettyName()})");
            }

            return naosOptions;
        }
    }
}
