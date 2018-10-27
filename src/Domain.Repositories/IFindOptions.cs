﻿namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Various options to specify the <see cref="IRepository{TEntity}"/> find operations
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IFindOptions<TEntity> // TODO: add WHERE T = IEntity?
    {
        int? Skip { get; set; }

        int? Take { get; set; }

        IEnumerable<Expression<Func<TEntity, object>>> Includes { get; set; }

        Func<TEntity, object> OrderBy { get; set; } // https://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet?rq=1
    }
}