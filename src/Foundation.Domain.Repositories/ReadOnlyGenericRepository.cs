﻿namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;

    public abstract class ReadOnlyGenericRepository<TEntity> : IReadOnlyGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IGenericRepository<TEntity> decoratee;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyGenericRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="decoratee">The decoratee.</param>
        protected ReadOnlyGenericRepository(IGenericRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.decoratee = decoratee;
        }

        /// <summary>
        /// Entity exists by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public virtual async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).AnyContext();
        }

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds one entitiy by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public virtual async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await this.decoratee.CountAsync(cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.CountAsync(specification, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(IEnumerable<ISpecification<TEntity>> specifications, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.CountAsync(specifications, cancellationToken).AnyContext();
        }
    }
}