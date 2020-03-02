﻿namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.FileStorage.Domain;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.Messaging;
    using Naos.Messaging.Application;
    using Naos.Messaging.Domain;
    using Naos.Messaging.Infrastructure;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static MessagingOptions UseFileStorageBroker(
            this MessagingOptions options,
            Action<IMessageBroker> brokerAction = null,
            string messageScope = null,
            string section = "naos:messaging:fileStorage")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var configuration = options.Context.Configuration.GetSection(section).Get<FileStorageConfiguration>();
            if (configuration?.Enabled == true)
            {
                options.Context.Services.AddScoped<IMessageBroker>(sp =>
                {
                    configuration.Folder = configuration.Folder.EmptyToNull() ?? Path.GetTempPath();
                    var broker = new FileStorageMessageBroker(o => o
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .Tracer(sp.GetService<ITracer>())
                        .Mediator((IMediator)sp.CreateScope().ServiceProvider.GetService(typeof(IMediator)))
                        .HandlerFactory(new ServiceProviderMessageHandlerFactory(sp))
                        .Storage(new FileStorageLoggingDecorator(
                            sp.GetRequiredService<ILoggerFactory>(),
                            new FolderFileStorage(s => s
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Folder(configuration.Folder)
                                .Serializer(new JsonNetSerializer()))))
                        .ProcessDelay(configuration.ProcessDelay)
                        .Map(sp.GetRequiredService<ISubscriptionMap>())
                        .FilterScope(Environment.GetEnvironmentVariable(EnvironmentKeys.IsLocal).ToBool()
                                ? Environment.MachineName.Humanize().Dehumanize().ToLower()
                                : string.Empty)
                        .MessageScope(messageScope));

                    brokerAction?.Invoke(broker);
                    return broker;
                });

                options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: messaging added (broker={nameof(FileStorageMessageBroker)})");
            }
            else
            {
                throw new NotImplementedException("no messaging filestorage is enabled");
            }

            return options;
        }
    }
}
