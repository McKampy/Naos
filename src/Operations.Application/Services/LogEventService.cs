﻿namespace Naos.Operations.Application
{
    //using System.Collections.Generic;
    //using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Operations.Domain;
    //using Naos.RequestFiltering.Application;

    public class LogEventService : ILogEventService
    {
        private readonly ILogger<LogEventService> logger;
        private readonly ILogEventRepository repository;

        public LogEventService(
            ILoggerFactory loggerFactory,
            ILogEventRepository repository)
        {
            EnsureThat.EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureThat.EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = loggerFactory.CreateLogger<LogEventService>();
            this.repository = repository;
        }

        //public async IAsyncEnumerable<string> GetLogEventsAsync(FilterContext filterContext)
        //{
        //    await Task.Delay(1000);
        //    yield return "xyz";
        //}
    }
}
