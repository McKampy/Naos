﻿namespace Naos.Core.App.Web
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static IMvcBuilder AddNaos(
            this IMvcBuilder mvcBuilder,
            Action<NaosMvcOptions> optionsAction = null)
        {
            var options = new NaosMvcOptions();
            optionsAction?.Invoke(options);

            if(!options.ControllerRegistrations.IsNullOrEmpty())
            {
                mvcBuilder
                    .AddMvcOptions(o =>
                    {
                        o.Filters.Add<OperationCancelledExceptionFilter>();
                        o.Conventions.Add(new GeneratedControllerRouteConvention());
                    })
                    .ConfigureApplicationPartManager(o => o
                        .FeatureProviders.Add(
                            new GeneratedRepositoryControllerFeatureProvider(options.ControllerRegistrations)));
            }

            mvcBuilder.AddControllersAsServices(); // needed to resolve controllers through di https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
            mvcBuilder.AddJsonOptions(o => o.AddDefaultJsonSerializerSettings(options.JsonSerializerSettings));

            return mvcBuilder;
        }
    }
}
