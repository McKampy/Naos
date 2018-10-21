﻿namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="maxItemCount">The maximum item count.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> FindAllAsync<T>(
            this IRepository<T> source,
            string tenantId,
            int maxItemCount = -1)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            return await source.FindAllAsync(
                HasTenantSpecification<T>.Factory.Create(tenantId),
                maxItemCount).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specification">The specification.</param>
        /// <param name="maxItemCount">The maximum item count.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> FindAllAsync<T>(
            this IRepository<T> source,
            string tenantId,
            Specification<T> specification,
            int maxItemCount = -1)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            EnsureArg.IsNotNull(specification);

            return await source.FindAllAsync(
                new List<Specification<T>>
                {
                    specification,
                    HasTenantSpecification<T>.Factory.Create(tenantId)
                },
                maxItemCount).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specifications">The specifications.</param>
        /// <param name="maxItemCount">The maximum item count.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> FindAllAsync<T>(
            this IRepository<T> source,
            string tenantId,
            IEnumerable<Specification<T>> specifications,
            int maxItemCount = -1)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            var specificationsArray = specifications as Specification<T>[] ?? specifications.ToArray();
            EnsureArg.IsNotNull(specificationsArray);
            EnsureArg.IsTrue(specificationsArray.Any());

            return await source.FindAllAsync(
                new List<Specification<T>>(specificationsArray)
                {
                    HasTenantSpecification<T>.Factory.Create(tenantId)
                },
                maxItemCount).ConfigureAwait(false);
        }
    }
}