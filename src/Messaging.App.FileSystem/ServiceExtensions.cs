﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.IO;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure.FileSystem;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.App;
    using Naos.Core.Messaging.App.Web;
    using Naos.Core.Messaging.Infrastructure;

    public static class ServiceExtensions
    {
        public static MessagingOptions AddFileSystemBroker(
            this MessagingOptions options,
            Action<IMessageBroker> setupAction = null,
            string messageScope = null,
            string section = "naos:messaging:fileSystem")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.Scan(scan => scan // https://andrewlock.net/using-scrutor-to-automatically-register-your-services-with-the-asp-net-core-di-container/
                .FromExecutingAssembly()
                .FromApplicationDependencies(a => !a.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) && !a.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase))
                .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)), true));

            options.Context.Services.AddSingleton<Hosting.IHostedService>(sp =>
                    new MessagingHostedService(sp.GetRequiredService<ILogger<MessagingHostedService>>(), sp));

            options.Context.Services.AddSingleton<ISubscriptionMap, SubscriptionMap>();
            options.Context.Services.AddSingleton<IMessageBroker>(sp =>
            {
                var fileSystemConfiguration = options.Context.Configuration.GetSection(section).Get<FileSystemConfiguration>();
                fileSystemConfiguration.Folder = fileSystemConfiguration.Folder.EmptyToNull() ?? Path.GetTempPath();
                var result = new FileSystemMessageBroker(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                    .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                    .Storage(new FileStorageLoggingDecorator(
                        sp.GetRequiredService<ILoggerFactory>(),
                        new FolderFileStorage(s => s
                            .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                            .Folder(fileSystemConfiguration.Folder)
                            .Serializer(new JsonNetSerializer()))))
                    .Configuration(fileSystemConfiguration)
                    .Map(sp.GetRequiredService<ISubscriptionMap>())
                    .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                            ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                            : string.Empty)
                    .MessageScope(messageScope));

                setupAction?.Invoke(result);
                return result;
            });

            return options;
        }
    }
}
