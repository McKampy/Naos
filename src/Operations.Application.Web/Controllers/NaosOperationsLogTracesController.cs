﻿namespace Naos.Operations.Application.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Foundation.Domain;
    using Naos.RequestFiltering.Application;
    using Naos.Tracing.Domain;
    using NSwag.Annotations;

    [Route("naos/operations/logtraces")]
    [ApiController]
    public class NaosOperationsLogTracesController : ControllerBase
    {
        private readonly ILogger<NaosOperationsLogTracesController> logger;
        private readonly FilterContext filterContext;
        private readonly ILogTraceRepository repository;
        private readonly ILogEventService service;
        private readonly ServiceDescriptor serviceDescriptor;
        private readonly ObjectPool<StringBuilder> stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();

        public NaosOperationsLogTracesController(
            ILoggerFactory loggerFactory,
            ILogTraceRepository repository,
            ILogEventService service,
            IFilterContextAccessor filterContext,
            ServiceDescriptor serviceDescriptor = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureArg.IsNotNull(service, nameof(service));

            this.logger = loggerFactory.CreateLogger<NaosOperationsLogTracesController>();
            this.filterContext = filterContext.Context ?? new FilterContext();
            this.repository = repository;
            this.service = service;
            this.serviceDescriptor = serviceDescriptor;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<IEnumerable<LogTrace>>> Get()
        {
            //var acceptHeader = this.HttpContext.Request.Headers.GetValue("Accept");
            //if (acceptHeader.ContainsAny(new[] { ContentType.HTML.ToValue(), ContentType.HTM.ToValue() }))
            //{
            //    return await this.GetHtmlAsync().AnyContext();
            //}

            return this.Ok(await this.GetJsonAsync().AnyContext());
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Operations")]
        public async Task<ActionResult<LogTrace>> Get(string id)
        {
            return this.Ok(await this.repository.FindOneAsync(id).AnyContext());
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

        private async Task<IEnumerable<LogTrace>> GetJsonAsync()
        {
            LoggingFilterContext.Prepare(this.filterContext); // add some default criteria

            return await this.repository.FindAllAsync(
                this.filterContext.GetSpecifications<LogTrace>().Insert(
                    new Specification<LogTrace>(t => t.TrackType == "trace")),
                this.filterContext.GetFindOptions<LogTrace>()).AnyContext();
        }

        private Task GetHtmlAsync()
        {
            this.HttpContext.Response.WriteNaosDashboard(
                title: this.serviceDescriptor?.ToString(),
                tags: this.serviceDescriptor?.Tags,
                action: async r =>
                {
                    var entities = this.GetJsonAsync().Result;
                    var nodes = Node<LogTrace>.ToHierarchy(entities, l => l.SpanId, l => l.ParentSpanId, true).ToList();

                    try
                    {
                        await nodes.RenderAsync(
                        t => this.WriteTrace(t),
                        t => this.WriteTraceHeader(t),
                        orderBy: t => t.Ticks,
                        options: new HtmlNodeRenderOptions(r.HttpContext) { ChildNodeBreak = string.Empty }).AnyContext();
                    }
                    catch
                    {
                        // do nothing
                    }

                    //foreach (var entity in entities) // .Where(l => !l.TrackType.EqualsAny(new[] { LogTrackTypes.Trace }))
                    //{
                    //    await this.WriteTraceAsync(entity).AnyContext();
                    //}
                }).Wait();

            return Task.CompletedTask;
        }

        private string WriteTraceHeader(LogTrace entity)
        {
            var sb = this.stringBuilderPool.Get(); // less allocations
            sb.Append("<div style='white-space: nowrap;'>")
                .Append("<span style='color: #EB1864; font-size: x-small;'>")
                .AppendFormat("{0:u}", entity.Timestamp.ToUniversalTime())
                .Append("</span>");
            sb.Append("&nbsp;[<span style='color: ")
                .Append(this.GetTraceLevelColor(entity)).Append("'>")
                .Append(entity.Kind?.ToUpper().Truncate(6, string.Empty))
                .Append("</span>]");
            sb.Append(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a style='font-size: xx-small;' target=\"blank\" href=\"/naos/operations/logevents/dashboard?q=CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            sb.Append($"<span style='color: #AE81FF; font-size: xx-small;'>{entity.ServiceName.Truncate(15, string.Empty, TruncateFrom.Left)}</span>&nbsp;");
            //sb.Append(!entity.CorrelationId.IsNullOrEmpty() ? $"&nbsp;<a target=\"blank\" href=\"/naos/operations/logtraces/dashboard?q=CorrelationId={entity.CorrelationId}\">{entity.CorrelationId.Truncate(12, string.Empty, Truncator.FixedLength, TruncateFrom.Left)}</a>&nbsp;" : "&nbsp;");

            var result = sb.ToString();
            this.stringBuilderPool.Return(sb);
            return result;
        }

        private string WriteTrace(LogTrace entity)
        {
            var extraStyles = string.Empty;
            var sb = this.stringBuilderPool.Get(); // less allocations
            sb.Append("<span style='color: ").Append(this.GetTraceLevelColor(entity)).Append("; ").Append(extraStyles).Append("'>");
            //.Append(logEvent.TrackType.SafeEquals("journal") ? "*" : "&nbsp;"); // journal prefix
            if (entity.Message?.Length > 5 && entity.Message.Take(6).All(char.IsUpper))
            {
                sb.Append($"<span style='color: #37CAEC;'>{entity.Message.Slice(0, 6)}</span>");
                sb.Append(entity.Message.Slice(6)).Append(" (").Append(entity.SpanId).Append("/").Append(entity.ParentSpanId).Append(")&nbsp;");
            }
            else
            {
                sb.Append(entity.Message).Append(" (").Append(entity.SpanId).Append("/").Append(entity.ParentSpanId).Append(")&nbsp;");
            }

            sb.Append("<a target='blank' href='/naos/operations/logtraces/").Append(entity.Id).Append("'>*</a> ");
            sb.Append("<span style='color: gray;font-size: xx-small'>-> took ");
            sb.Append(entity.Duration.Humanize());
            sb.Append("</span>");
            sb.Append("</span>");
            sb.Append("</div>");

            var result = sb.ToString();
            this.stringBuilderPool.Return(sb);
            return result;
        }

        private string GetTraceLevelColor(LogTrace entity)
        {
            var levelColor = "#96E228";
            if (entity.Status.SafeEquals(nameof(SpanStatus.Transient)))
            {
                levelColor = "#75715E";
            }
            else if (entity.Status.SafeEquals(nameof(SpanStatus.Cancelled)))
            {
                levelColor = "#FF8C00";
            }
            else if (entity.Status.SafeEquals(nameof(SpanStatus.Failed)))
            {
                levelColor = "#FF0000";
            }

            return levelColor;
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}
