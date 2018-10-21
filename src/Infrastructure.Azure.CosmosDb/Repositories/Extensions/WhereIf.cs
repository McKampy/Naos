﻿namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class Extensions
    {
        public static IQueryable<T> WhereExpressionIf<T>(
            this IQueryable<T> source,
            bool condition,
            Expression<Func<T, bool>> expression)
        {
            if (condition && expression != null)
            {
                return source.Where(expression);
            }

            return source;
        }

        public static IQueryable<T> WhereExpressionsIf<T>(
            this IQueryable<T> source,
            bool condition,
            IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            if (condition && expressions?.Any() == true)
            {
                foreach (var expression in expressions)
                {
                    source = source.Where(expression);
                }
            }

            return source;
        }
    }
}
