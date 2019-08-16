﻿namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    //using System.IO;
    //using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    //using Microsoft.Azure.Documents;

    public interface ICosmosDbSqlProvider<T>
    {
        Task<T> GetByIdAsync(string id, string partitionKey = null);

        Task<T> UpsertAsync(T entity, string partitionKeyValue = null);

        Task<IEnumerable<T>> WhereAsync(
            Expression<Func<T, bool>> expression,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            string partitionKeyValue = null);

        Task<IEnumerable<T>> WhereAsync(
            IEnumerable<Expression<Func<T, bool>>> expressions = null,
            string partitionKeyValue = null,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false);

        Task<IEnumerable<T>> WhereAsync( // OBSOLETE
            Expression<Func<T, bool>> expression,
            Expression<Func<T, T>> selector,
            int? skip = null,
            int? take = null,
            Expression<Func<T, object>> orderExpression = null,
            bool orderDescending = false,
            string partitionKeyValue = null);

        Task<bool> DeleteByIdAsync(string id, string partitionKeyValue = null);

        //Task<T> UpsertAttachmentAsync(T entity, string attachmentId, string contentType, Stream stream);

        //Task<int> CountAsync();

        //Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression = null);

        //Task<IEnumerable<T>> GetAllAsync(int count = -1);

        //IEnumerable<string> GetAllIdsBatched(int count = 100);

        //IEnumerable<T> GetAllBatched(int count = 100);

        //Task<Attachment> GetAttachmentByIdAsync(string id, string attachmentId);

        //Task<IEnumerable<string>> GetAttachmentIdsAsync(string id);

        //Task<IEnumerable<Attachment>> GetAttachmentsAsync(string id);

        //Task<Stream> GetAttachmentStreamByIdAsync(string id, string attachmentId);

        //Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> expression = null);

        //Task<IQueryable<T>> QueryAsync(int count = 100);

        //Task<IEnumerable<T>> QueryAsync(string query);

        //Task<bool> DeleteAllAsync();
    }
}