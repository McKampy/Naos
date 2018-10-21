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
        /// <typeparam name="TEntity">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="maxItemCount">The maximum item count.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(
            this IRepository<TEntity> source,
            string tenantId,
            int maxItemCount = -1)
            where TEntity : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            return await source.FindAllAsync(
                HasTenantSpecification<TEntity>.Factory.Create(tenantId),
                maxItemCount).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specification">The specification.</param>
        /// <param name="maxItemCount">The maximum item count.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(
            this IRepository<TEntity> source,
            string tenantId,
            Specification<TEntity> specification,
            int maxItemCount = -1)
            where TEntity : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            EnsureArg.IsNotNull(specification);

            return await source.FindAllAsync(
                new List<Specification<TEntity>>
                {
                    specification,
                    HasTenantSpecification<TEntity>.Factory.Create(tenantId)
                },
                maxItemCount).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specifications">The specifications.</param>
        /// <param name="maxItemCount">The maximum item count.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(
            this IRepository<TEntity> source,
            string tenantId,
            IEnumerable<Specification<TEntity>> specifications,
            int maxItemCount = -1)
            where TEntity : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            var specificationsArray = specifications as Specification<TEntity>[] ?? specifications.ToArray();
            EnsureArg.IsNotNull(specificationsArray);
            EnsureArg.IsTrue(specificationsArray.Any());

            return await source.FindAllAsync(
                new List<Specification<TEntity>>(specificationsArray)
                {
                    HasTenantSpecification<TEntity>.Factory.Create(tenantId)
                },
                maxItemCount).ConfigureAwait(false);
        }
    }
}
