﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Naos.Core.Commands.App;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Configuration.App;
    using Naos.Foundation;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the command handling functionality.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="optionsAction"></param>
        public static NaosServicesContextOptions AddCommands(
            this NaosServicesContextOptions naosOptions,
            Action<CommandsOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            // needed for mediator, register command behaviors
            naosOptions.Context.Services
                .Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                    .FromExecutingAssembly()
                    .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandBehavior)), true));

            // needed for mediator, register all commands + handlers
            naosOptions.Context.Services
                .Scan(scan => scan
                    .FromApplicationDependencies()
                    .AddClasses(classes => classes.Where(c => (c.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) || c.Name.EndsWith("CommandHandler", StringComparison.OrdinalIgnoreCase)) && !c.Name.Contains("ConsoleCommand")))
                    .AsImplementedInterfaces());

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: commands added"); // TODO: list available commands/handlers

            optionsAction?.Invoke(new CommandsOptions(naosOptions.Context));
            //naosOptions.Context.Services
            //    .AddSingleton<ICommandBehavior, ValidateCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, TrackCommandBehavior>()
            //    //.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, IdempotentCommandBehavior>()
            //    .AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Commands", EchoRoute = "api/echo/commands" });

            return naosOptions;
        }

        public static CommandsOptions AddBehavior<TBehavior>(
            this CommandsOptions options)
            where TBehavior : class, ICommandBehavior
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<ICommandBehavior, TBehavior>();

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: commands behavior added (type={typeof(TBehavior).Name})"); // TODO: list available commands/handlers

            return options;
        }

        public static CommandsOptions AddBehavior<TBehavior>(
            this CommandsOptions options,
            TBehavior behavior)
            where TBehavior : class, ICommandBehavior
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));
            EnsureArg.IsNotNull(behavior, nameof(behavior));

            options.Context.Services.AddSingleton<ICommandBehavior>(behavior);

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: commands behavior added (type={typeof(TBehavior).Name})"); // TODO: list available commands/handlers

            return options;
        }

        public static CommandsOptions AddRequestDispatcher(
            this CommandsOptions options,
            Action<RequestDispatcherOptions> optionsAction = null,
            bool addDefaultRequestCommands = true)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            if (addDefaultRequestCommands)
            {
                options.Context.Services.AddSingleton<RequestCommandRegistration>(sp => new RequestCommandRegistration<EchoCommand, EchoCommandResponse> { Route = "/api/commands/echo", RequestMethod = "get" });
                options.Context.Services.AddSingleton<RequestCommandRegistration>(sp => new RequestCommandRegistration<EchoCommand, EchoCommandResponse> { Route = "/api/commands/echo", RequestMethod = "post" });
                options.Context.Services.AddSingleton<RequestCommandRegistration>(sp => new RequestCommandRegistration<PingCommand> { Route = "/api/commands/ping", RequestMethod = "get" });
            }

            optionsAction?.Invoke(new RequestDispatcherOptions(options.Context));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: command request dispatcher added"); // TODO: list available command + routes

            return options;
        }
    }
}
