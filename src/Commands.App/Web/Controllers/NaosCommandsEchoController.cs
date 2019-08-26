﻿namespace Naos.Core.Commands.App.Web
{
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;

    [Route("api/echo/commands")]
    [ApiController]
    public class NaosCommandsEchoController : ControllerBase
    {
        private readonly ILogger<NaosCommandsEchoController> logger;
        private readonly IEnumerable<RequestCommandRegistration> registrations;

        public NaosCommandsEchoController(
            ILogger<NaosCommandsEchoController> logger,
            IEnumerable<RequestCommandRegistration> registrations)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.registrations = registrations;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        public ActionResult<IEnumerable<RequestCommandRegistration>> Get()
        {
            return this.Ok(this.registrations);
        }
    }
}
