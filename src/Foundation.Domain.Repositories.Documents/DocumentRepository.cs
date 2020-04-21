﻿namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class DocumentRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly DocumentRepositoryOptions<TEntity> options;

        public DocumentRepository(DocumentRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));

            this.options = options;
            this.Logger = options.CreateLogger<DocumentRepository<TEntity>>();

            this.Logger.LogInformation($"{{LogKey:l}} construct sql document repository (type={typeof(TEntity).PrettyName()})", LogKeys.DomainRepository);
        }

        public DocumentRepository(Builder<DocumentRepositoryOptionsBuilder<TEntity>, DocumentRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new DocumentRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        public ILogger<DocumentRepository<TEntity>> Logger { get; }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return RepositoryActionResult.None;
            }

            var result = await this.options.Provider.DeleteAsync(id).AnyContext();
            if (result == ProviderAction.Deleted)
            {
                return RepositoryActionResult.Deleted;
            }

            return RepositoryActionResult.None;
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            if (entity == null)
            {
                return RepositoryActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.options.Provider.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (specification == null)
            {
                return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
            }

            return await this.FindAllAsync(new[] { specification }, options, cancellationToken).AnyContext();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return this.options.Provider.LoadValuesAsync(
                expressions: specifications?.Select(s => s.ToExpression()),
                skip: options?.Skip,
                take: options?.Take,
                orderExpression: options?.Order?.Expression,
                orderDescending: options?.Order?.Direction == OrderDirection.Descending)?.ToEnumerable();

            //var results = new List<TEntity>();

            //var rs = this.options.Provider.LoadValuesAsync(
            //    expressions: specifications?.Select(s => s.ToExpression()),
            //    skip: options?.Skip,
            //    take: options?.Take,
            //    orderExpression: options?.Order?.Expression,
            //    orderDescending: options?.Order?.Direction == OrderDirection.Descending);

            //await foreach (var r in rs)
            //{
            //    results.Add(r);
            //}

            //return results;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<TEntity> FindOneAsync(object id)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (id.IsDefault())
            {
                return null;
            }

            return this.options.Provider.LoadValuesAsync(id).ToEnumerable().FirstOrDefault();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return (await this.UpsertAsync(entity).AnyContext()).entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return (await this.UpsertAsync(entity).AnyContext()).entity;
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (null, RepositoryActionResult.None);
            }

            var isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).AnyContext();

            if (entity.Id.IsDefault())
            {
                this.options.IdGenerator.SetNew(entity);
            }

            if (this.options.PublishEvents && this.options.Mediator != null)
            {
                if (isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
                }
            }

            if (isNew)
            {
                if (entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }
            }

            this.Logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            var result = await this.options.Provider.UpsertAsync(entity.Id, entity).AnyContext();

            if (this.options.PublishEvents && this.options.Mediator != null)
            {
                if (isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertedDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdatedDomainEvent(entity)).AnyContext();
                }
            }

            return result switch
            {
                ProviderAction.Inserted => (entity, RepositoryActionResult.Inserted),
                ProviderAction.Updated => (entity, RepositoryActionResult.Updated),
                _ => (entity, RepositoryActionResult.None)
            };
        }
    }
}
