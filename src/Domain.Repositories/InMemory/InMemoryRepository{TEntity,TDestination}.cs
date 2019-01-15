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
        private readonly IEnumerable<ISpecificationMapper<TEntity, TDestination>> specificationMappers;
        private readonly Func<TDestination, object> idSelector;

        public InMemoryRepository(
            IMediator mediator,
            Func<TDestination, object> idSelector,
            InMemoryContext<TEntity> context,
            IRepositoryOptions options = null,
            IEnumerable<ISpecificationMapper<TEntity, TDestination>> specificationMappers = null)
            : base(mediator, context, options)
        {
            EnsureArg.IsNotNull(idSelector, nameof(idSelector));
            EnsureArg.IsNotNull(options?.Mapper, nameof(options.Mapper));

            //base.entities = entities.NullToEmpty().Select(d => this.Options.Mapper.Map<TEntity>(d));
            this.specificationMappers = specificationMappers;
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
            var result = this.context.Entities.NullToEmpty().Select(d => this.Options.Mapper.Map<TDestination>(d)); // work on destination objects

            foreach (var specification in specifications.NullToEmpty())
            {
                result = result.Where(this.EnsurePredicate(specification)); // translate specification to destination predicate
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
                return default;
            }

            var result = this.context.Entities.NullToEmpty().Select(d => this.Options.Mapper.Map<TDestination>(d)) // work on destination objects
                .SingleOrDefault(e => this.idSelector(e).Equals(id)); // TODO: use HasIdSpecification + MapExpression (makes idSelector obsolete)
            // return (await this.FindAllAsync(new HasIdSpecification<TEntity>(id))).FirstOrDefault();

            if (this.Options?.Mapper != null && result != null)
            {
                return await Task.FromResult(this.Options.Mapper.Map<TEntity>(result));
            }

            return default;
        }

        protected new Func<TDestination, bool> EnsurePredicate(ISpecification<TEntity> specification)
        {
            foreach(var specificationMapper in this.specificationMappers.NullToEmpty())
            {
                if (specificationMapper.CanHandle(specification))
                {
                    return specificationMapper.Map(specification);
                }
            }

            throw new NaosException($"no applicable specification mapper found for {specification.GetType().PrettyName()}");
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

            foreach(var orderBy in options?.OrderBy.NullToEmpty())
            {
                if(orderBy.Direction == OrderByDirection.Ascending)
                {
                    result = result.OrderBy(
                        this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(orderBy.Expression).Compile());
                }
                else
                {
                    result = result.OrderByDescending(
                        this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(orderBy.Expression).Compile());
                }
            }

            if (this.Options?.Mapper != null && result != null)
            {
                return result.Select(d => this.Options.Mapper.Map<TEntity>(d));
            }

            return null;
        }
    }
}