﻿namespace Naos.Core.RequestFiltering.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;

    /// <inheritdoc />
    public class FilterContextFactory : IFilterContextFactory
    {
        private readonly ILogger<FilterContextFactory> logger;
        private readonly IFilterContextAccessor accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory" /> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        public FilterContextFactory(ILogger<FilterContextFactory> logger)
            : this(logger, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterContextFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="accessor">The <see cref="IFilterContextAccessor"/> through which the <see cref="FilterContext"/> will be set.</param>
        public FilterContextFactory(ILogger<FilterContextFactory> logger, IFilterContextAccessor accessor)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.accessor = accessor;
        }

        /// <inheritdoc />
        public FilterContext Create(HttpRequest request, string criteriaQueryStringKey, string orderByQueryStringKey, string skipQueryStringKey, string takeQueryStringKey)
        {
            var result = new FilterContext
            {
                Criterias = this.BuildCriterias(request, criteriaQueryStringKey),
                Orders = this.BuildOrders(request, orderByQueryStringKey),
                Skip = request?.Query?.FirstOrDefault(p => p.Key.Equals(skipQueryStringKey, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault().ToNullableInt(),
                Take = request?.Query?.FirstOrDefault(p => p.Key.Equals(takeQueryStringKey, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault().ToNullableInt()
            };

            if (this.accessor != null)
            {
                this.accessor.Context = result;
            }

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.accessor != null)
            {
                this.accessor.Context = null;
            }
        }

        private IEnumerable<Criteria> BuildCriterias(HttpRequest request, string criteriaQueryStringKey)
        {
            if (request?.Query?.ContainsKey(criteriaQueryStringKey) == false)
            {
                return Enumerable.Empty<Criteria>();
            }

            // correlationId=eq:2b34cc25-cd06-475c-8f9c-c42791f49b46,timestamp=qte:01-01-1980,level=eq:debug,OR,level=eq:information
            var query = request.Query.FirstOrDefault(p => p.Key.Equals(criteriaQueryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Criteria>();
            foreach (var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                if (item.EqualsAny(new[] { "and", "or" }))
                {
                    // TODO: AND / OR
                    continue;
                }

                var name = item.SubstringTill("=");
                var value = item.SubstringFrom("=");
                var @operator = value.Contains(":") ? value.SubstringTill(":").Trim() : "eq";

                result.Add(
                    new Criteria(
                        name.Trim(),
                        CriteriaOperatorExtensions.FromAbbreviation(@operator),
                        (value.Contains(":") ? value.SubstringFrom(":") : value).Trim().EmptyToNull()));
                        // TODO: properly determine numeric oder not and pass to criteria
            }

            if (result.Count > 0)
            {
                this.logger.LogDebug($"{{LogKey:l}} [{request.HttpContext.GetRequestId()}] http filter criterias={result.Select(c => c.ToString()).ToString("|")}", LogEventKeys.InboundRequest);
            }

            return result;
        }

        private IEnumerable<Order> BuildOrders(HttpRequest request, string orderByQueryStringKey)
        {
            if (request?.Query?.ContainsKey(orderByQueryStringKey) == false)
            {
                return Enumerable.Empty<Order>();
            }

            // order=desc:timestamp,level
            var query = request.Query.FirstOrDefault(p => p.Key.Equals(orderByQueryStringKey, StringComparison.OrdinalIgnoreCase));
            var items = query.Value.ToString().Split(',');

            var result = new List<Order>();
            foreach (var item in items.Where(c => !c.IsNullOrEmpty()))
            {
                var name = item.Contains(":") ? item.SubstringFrom(":").Trim() : item;
                var direction = item.Contains(":") ? item.SubstringTill(":").Trim() : "ascending";

                result.Add(
                    new Order(
                        name.Trim(),
                        Enum.TryParse(direction, true, out OrderDirection e) ? e : OrderDirection.Asc));
            }

            if (result.Count > 0)
            {
                this.logger.LogDebug($"{{LogKey:l}} [{request.HttpContext.GetRequestId()}] http filter orders={result.Select(o => o.ToString()).ToString("|")}", LogEventKeys.InboundRequest);
            }

            return result;
        }

        //private bool IsNumeric(string value) => value.Safe().All(char.IsDigit);
    }
}