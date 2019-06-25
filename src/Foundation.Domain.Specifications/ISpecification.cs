﻿namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;

    public interface ISpecification<T>
    {
        string Name { get; set; }

        Expression<Func<T, bool>> ToExpression();

        Func<T, bool> ToPredicate();

        bool IsSatisfiedBy(T entity);

        ISpecification<T> Or(ISpecification<T> specification);

        ISpecification<T> And(ISpecification<T> specification);

        ISpecification<T> Not();

        string ToString();

        string ToString(bool noBrackets);
    }
}