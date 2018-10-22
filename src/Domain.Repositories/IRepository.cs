﻿namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Specifications;

    public interface IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        Task<IEnumerable<T>> FindAllAsync(IFindOptions<T> options = null);

        Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, IFindOptions<T> options = null);

        Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, IFindOptions<T> options = null);

        Task<T> FindOneAsync(object id);

        //Task<T> FindOneAsync(ISpecification<T> specification);

        Task<bool> ExistsAsync(object id);

        Task<T> AddOrUpdateAsync(T entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(T entity);
    }
}