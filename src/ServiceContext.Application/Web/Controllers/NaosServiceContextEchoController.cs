﻿namespace Naos.ServiceContext.Application.Web
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using NSwag.Annotations;

    [Route("naos/servicecontext/echo")]
    [ApiController]
    public class NaosServiceContextEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosServiceContextEchoController> logger;
        private readonly ServiceDescriptor serviceContext;

        public NaosServiceContextEchoController(
            ILogger<NaosServiceContextEchoController> logger,
            ServiceDescriptor serviceContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.serviceContext = serviceContext;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        public ActionResult<ServiceDescriptor> Get()
        {
            return this.Ok(this.serviceContext);
        }
    }
}
