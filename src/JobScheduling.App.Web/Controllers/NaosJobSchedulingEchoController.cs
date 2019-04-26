﻿namespace Naos.Core.JobScheduling.App.Web
{
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling.Domain;
    using NSwag.Annotations;

    [Route("api/echo/jobscheduling")]
    [ApiController]
    public class NaosJobSchedulingEchoController : ControllerBase
    {
        private readonly ILogger<NaosJobSchedulingEchoController> logger;
        private readonly IJobScheduler jobScheduler;

        public NaosJobSchedulingEchoController(
            ILogger<NaosJobSchedulingEchoController> logger,
            IJobScheduler jobScheduler)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(jobScheduler, nameof(jobScheduler));

            this.logger = logger;
            this.jobScheduler = jobScheduler;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerTag("Naos Echo")]
        public ActionResult<IEnumerable<JobRegistration>> Get()
        {
            return this.Ok(this.jobScheduler.Options.Registrations.Keys);
        }
    }
}
