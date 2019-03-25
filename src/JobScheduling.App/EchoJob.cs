﻿namespace Naos.Core.JobScheduling.App
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class EchoJob
    {
        private static readonly Random Random = new Random(DateTime.Now.GetHashCode());
        private readonly ILogger<EchoJob> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public EchoJob(ILogger<EchoJob> logger, IHttpClientFactory httpClientFactory)
        {
            EnsureThat.EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureThat.EnsureArg.IsNotNull(httpClientFactory, nameof(httpClientFactory));

            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task EchoAsync(string message, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (!message.IsNullOrEmpty())
                {
                    this.logger.LogInformation(message);
                }
            }, cancellationToken);
        }

        public Task EchoAsync(string message, CancellationToken cancellationToken, bool breakable)
        {
            if (breakable && Random.Next(2) == 0)// throw randomly
            {
                throw new NaosException("error from job");
            }

            if (!message.IsNullOrEmpty())
            {
                this.logger.LogInformation(message);
            }

            return Task.CompletedTask;
        }

        public async Task EchoLongAsync(string message, CancellationToken cancellationToken)
        {
            for (int i = 1; i <= 5; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation("job cancelled");
                    return; //Task.FromCanceled(cancellationToken);
                }

                if (!message.IsNullOrEmpty())
                {
                    this.logger.LogInformation(message);
                }

                // TODO: use typed client here
                var httpClient = this.httpClientFactory.CreateClient("default"); // contains the configured handlers
                var response = await httpClient.PostAsync("http://mockbin.org/request", null, cancellationToken).AnyContext();

                Thread.Sleep(new TimeSpan(0, 0, 45));
                //await Task.Delay(new TimeSpan(0, 0, 45));
            }
        }
    }
}
