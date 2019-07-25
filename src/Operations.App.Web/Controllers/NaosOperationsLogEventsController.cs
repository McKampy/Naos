﻿namespace Naos.Core.Operations.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;
    using Naos.Core.RequestFiltering.App;
    using Naos.Foundation;
    using NSwag.Annotations;

    [Route("api/operations/logevents")]
    [ApiController]
    public class NaosOperationsLogEventsController : ControllerBase
    {
        private readonly ILogger<NaosOperationsLogEventsController> logger;
        private readonly FilterContext filterContext;
        private readonly ILogEventRepository repository;
        private readonly ILogEventService service;

        public NaosOperationsLogEventsController(
            ILoggerFactory loggerFactory,
            ILogEventRepository repository,
            ILogEventService service,
            IFilterContextAccessor filterContext)
        {
            EnsureThat.EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureThat.EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureThat.EnsureArg.IsNotNull(service, nameof(service));

            this.logger = loggerFactory.CreateLogger<NaosOperationsLogEventsController>();
            this.filterContext = filterContext.Context ?? new FilterContext();
            this.repository = repository;
            this.service = service;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<IEnumerable<LogEvent>>> Get()
        {
            //var acceptHeader = this.HttpContext.Request.Headers.GetValue("Accept");
            //if (acceptHeader.ContainsAny(new[] { ContentType.HTML.ToValue(), ContentType.HTM.ToValue() }))
            //{
            //    return await this.GetHtmlAsync().AnyContext();
            //}

            return this.Ok(await this.GetJsonAsync().AnyContext());
        }

        [HttpGet]
        [Route("dashboard")]
        [Produces("text/html")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public Task GetHtml()
        {
            return this.GetHtmlAsync();
        }

        private async Task<IEnumerable<LogEvent>> GetJsonAsync()
        {
            this.EnsureFilterContext();

            return await this.repository.FindAllAsync(
                this.filterContext.GetSpecifications<LogEvent>(),
                this.filterContext.GetFindOptions<LogEvent>()).AnyContext();
        }

        private async Task GetHtmlAsync()
        {
            this.HttpContext.Response.ContentType = "text/html";
            await this.HttpContext.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width' />
    <title>Naos</title>
    <base href='/' />
    <link rel='stylesheet' href='https://use.fontawesome.com/releases/v5.0.10/css/all.css' integrity='sha384-+d0P83n9kaQMCwj8F4RJB66tzIwOKmrdb46+porD/OvrJ+37WqIM7UoBtwHO6Nlg' crossorigin='anonymous'>
    <link href='css/naos.css' rel ='stylesheet' />
</head>
<body>
    <pre style='color: cyan;font-size: xx-small;'>
    " + ResourcesHelper.GetLogoAsString() + @"
    </pre>
    <hr />
    &nbsp;&nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/health'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard?q=TrackType=trace'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a>&nbsp;&nbsp;&nbsp;</br>
"); // TODO: reuse from ServiceContextMiddleware.cs
            try
            {
                this.EnsureFilterContext();

                var entities = await this.repository.FindAllAsync(
                    this.filterContext.GetSpecifications<LogEvent>(),
                    this.filterContext.GetFindOptions<LogEvent>()).AnyContext();

                foreach(var entity in entities
                    .Where(l => !l.TrackType.EqualsAny(new[] { LogTrackTypes.Trace })))
                {
                    var levelColor = "lime";
                    if(entity.Level.SafeEquals(nameof(LogLevel.Trace)) || entity.Level.SafeEquals(nameof(LogLevel.Debug)))
                    {
                        levelColor = "#75715E";
                    }
                    else if(entity.Level.SafeEquals(nameof(LogLevel.Warning)))
                    {
                        levelColor = "#FF8C00";
                    }
                    else if(entity.Level.SafeEquals(nameof(LogLevel.Critical)) || entity.Level.SafeEquals(nameof(LogLevel.Error)))
                    {
                        levelColor = "#FF0000";
                    }

                    var messageColor = levelColor;
                    var extraStyles = string.Empty;

                    await this.HttpContext.Response.WriteAsync("<div style='white-space: nowrap;'><span style='color: #EB1864; font-size: x-small;'>");
                    await this.HttpContext.Response.WriteAsync($"{entity.Timestamp.ToUniversalTime():u}");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync($"&nbsp;[<span style='color: {levelColor}'>");
                    await this.HttpContext.Response.WriteAsync($"{entity.Level.ToUpper().Truncate(3, string.Empty)}</span>]");
                    await this.HttpContext.Response.WriteAsync(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/api/operations/logevents/dashboard?q=CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;");
                    await this.HttpContext.Response.WriteAsync($"<span style='color: {messageColor}; {extraStyles}'>");
                    //await this.HttpContext.Response.WriteAsync(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
                    await this.HttpContext.Response.WriteAsync($"{entity.Message} <a target=\"blank\" href=\"/api/operations/logevents?q=Id={entity.Id}\">*</a>");
                    await this.HttpContext.Response.WriteAsync("</span>");
                    await this.HttpContext.Response.WriteAsync("</div>");
                }
            }
            finally
            {
                await this.HttpContext.Response.WriteAsync("</body></html>");
            }
        }

        private void EnsureFilterContext()
        {
            // environment (default: current environment)
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Environment))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Environment), CriteriaOperator.Equal, Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production"));
            }

            // message
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Message))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Message), CriteriaOperator.NotEqual, string.Empty));
            }

            // level (default: >= Information)
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Level))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
            }
            else
            {
                var criteria = this.filterContext.Criterias.FirstOrDefault(c => c.Name.SafeEquals(nameof(LogEvent.Level)));
                if(criteria.Value != null)
                {
                    if(criteria.Value.ToString().SafeEquals(nameof(LogLevel.Debug)))
                    {
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                    }
                    else if(criteria.Value.ToString().SafeEquals(nameof(LogLevel.Information)))
                    {
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
                    }
                    else if(criteria.Value.ToString().SafeEquals(nameof(LogLevel.Warning)))
                    {
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Information)));
                    }
                    else if(criteria.Value.ToString().SafeEquals(nameof(LogLevel.Error))
                        || criteria.Value.ToString().SafeEquals(nameof(LogLevel.Critical)))
                    {
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Trace)));
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Debug)));
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Information)));
                        this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Level), CriteriaOperator.NotEqual, nameof(LogLevel.Warning)));
                    }
                }
            }

            // time range (default: last 7 days)
            if(!this.filterContext.Criterias.SafeAny(c => c.Name.SafeEquals(nameof(LogEvent.Ticks))))
            {
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Ticks), CriteriaOperator.LessThanOrEqual, DateTime.UtcNow.Ticks));
                this.filterContext.Criterias = this.filterContext.Criterias.Insert(new Criteria(nameof(LogEvent.Ticks), CriteriaOperator.GreaterThanOrEqual, DateTime.UtcNow.AddHours(-24 * 7).Ticks));
            }

            //foreach(var criteria in this.filterContext.Criterias)
            //{
            //    await this.HttpContext.Response.WriteAsync($"criteria: {criteria}<br/>");
            //}

            this.filterContext.Take ??= 1000; // get amount per request, repeat while logevents.ticks >= past

            //await foreach(var name in this.service.GetLogEventsAsync(this.filterContext))
            //{
            //    this.logger.LogInformation(name);
            //}
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
