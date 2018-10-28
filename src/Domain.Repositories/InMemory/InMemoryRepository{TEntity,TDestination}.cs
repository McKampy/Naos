﻿namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// Represents an InMemoryRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the domain entity</typeparam>
    /// <typeparam name="TDestination">The type of the destination/remote dto.</typeparam>
    /// <seealso cref="Domain.InMemoryRepository{T}" />
    public class InMemoryRepository<TEntity, TDestination> : InMemoryRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        protected new IEnumerable<TDestination> entities;
        private readonly IEnumerable<ISpecificationTranslator<TEntity, TDestination>> specificationTranslators;
        private readonly Func<TDestination, object> idSelector;

        public InMemoryRepository(
            IMediator mediator,
            IEnumerable<TDestination> entities = null,
            IRepositoryOptions options = null,
            IEnumerable<ISpecificationTranslator<TEntity, TDestination>> specificationTranslators = null,
            Func<TDestination, object> idSelector = null)
            : base(mediator, options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Mapper, nameof(options.Mapper));

            this.entities = entities.NullToEmpty();
            this.specificationTranslators = specificationTranslators;
            this.idSelector = idSelector;
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public override async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            var result = this.entities;

            foreach (var specification in specifications.NullToEmpty())
            {
                result = result.Where(this.EnsurePredicate(specification));
            }

            return await Task.FromResult(this.FindAll(result, options));
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public override async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            var result = this.entities.SingleOrDefault(e => this.idSelector(e) == id);

            if (this.Options?.Mapper != null && result != null)
            {
                return await Task.FromResult(this.Options.Mapper.Map<TEntity>(result));
            }

            return null;
        }

        protected new Func<TDestination, bool> EnsurePredicate(ISpecification<TEntity> specification)
        {
            foreach(var translator in this.specificationTranslators.NullToEmpty())
            {
                if (translator.CanHandle(specification))
                {
                    return translator.Translate(specification);
                }
            }

            throw new NaosException($"no applicable specification translator found for {specification.GetType().PrettyName()}");
        }

        protected IEnumerable<TEntity> FindAll(IEnumerable<TDestination> entities, IFindOptions<TEntity> options = null)
        {
            var result = entities;

            if (options?.Skip.HasValue == true && options.Skip.Value > 0)
            {
                result = result.Skip(options.Skip.Value);
            }

            if (options?.Take.HasValue == true && options.Take.Value > 0)
            {
                result = result.Take(options.Take.Value);
            }

            if (options?.OrderBy != null)
            {
                result = result.OrderBy(
                    this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(options.OrderBy).Compile());
            }

            if (this.Options?.Mapper != null && result != null)
            {
                return result.Select(r => this.Options.Mapper.Map<TEntity>(r));
            }

            return null;
        }
    }
}