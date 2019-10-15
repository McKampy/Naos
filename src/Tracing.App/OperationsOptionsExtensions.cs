﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Operations;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class OperationsOptionsExtensions
    {
        public static OperationsOptions AddTracing(
            this OperationsOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: tracing added");
            options.Context.Services.AddScoped<ITracer>(sp =>
            {
                return new Tracer(
                    new AsyncLocalScopeManager((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator))),
                    sp.GetService<ISampler>());
            });
            options.Context.Services.AddSingleton<ISampler, ConstantSampler>(); // TODO: configure different samplers
            //options.Context.Services.AddSingleton<ISampler>(sp => new OperationNamePatternSampler(new[] { "http*" })); // TODO: configure different samplers
            //options.Context.Services.AddSingleton<ISampler>(sp => new RateLimiterSampler(new RateLimiter(2.0, 2.0))); // TODO: configure different samplers

            return options;
        }
    }
}