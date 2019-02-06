﻿namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class FileStorageLoggingDecorator : IFileStorage
    {
        private readonly ILogger<FileStorageLoggingDecorator> logger;

        public FileStorageLoggingDecorator(ILogger<FileStorageLoggingDecorator> logger, IFileStorage decoratee)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.logger = logger;
            this.Decoratee = decoratee;
        }

        public ISerializer Serializer => this.Decoratee.Serializer;

        private IFileStorage Decoratee { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey}} get file stream: {path}", LogEventKeys.FileStorage);
            return this.Decoratee.GetFileStreamAsync(path, cancellationToken);
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey}} get file info: {path}", LogEventKeys.FileStorage);
            return await this.GetFileInformationAsync(path);
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey}} exists file: {path}", LogEventKeys.FileStorage);
            return this.Decoratee.ExistsAsync(path);
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            this.logger.LogInformation($"{{LogKey}} save file: {path}", LogEventKeys.FileStorage);
            return this.Decoratee.SaveFileAsync(path, stream, cancellationToken);
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            this.logger.LogInformation($"{{LogKey}} rename file: {path} > {newPath}", LogEventKeys.FileStorage);
            return this.Decoratee.RenameFileAsync(path, newPath, cancellationToken);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            this.logger.LogInformation($"{{LogKey}} copy file: {path} > {targetPath}", LogEventKeys.FileStorage);
            return this.Decoratee.CopyFileAsync(path, targetPath, cancellationToken);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey}} delete file: {path}", LogEventKeys.FileStorage);
            return this.Decoratee.DeleteFileAsync(path, cancellationToken);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey}} delete file: {searchPattern}", LogEventKeys.FileStorage);
            return this.Decoratee.DeleteFilesAsync(searchPattern, cancellationToken);
        }

        public async Task<PagedResults> GetPagedFileListAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey}} get files: {searchPattern}", LogEventKeys.FileStorage);
            return await this.GetPagedFileListAsync(pageSize, searchPattern, cancellationToken);
        }

        public void Dispose()
        {
            this.Decoratee?.Dispose();
        }
    }
}
