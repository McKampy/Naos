﻿namespace Naos.Core.UnitTests.FileStorage
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using NSubstitute;
    using Xunit;

    public class ScopedFileStorageDecoratorTests : FileStorageTestsBase
    {
        [Fact]
        public override Task CanGetEmptyFileListOnMissingDirectoryAsync()
        {
            return base.CanGetEmptyFileListOnMissingDirectoryAsync();
        }

        [Fact]
        public override Task CanGetFileListForSingleFolderAsync()
        {
            return base.CanGetFileListForSingleFolderAsync();
        }

        [Fact]
        public override Task CanGetFileInfoAsync()
        {
            return base.CanGetFileInfoAsync();
        }

        [Fact]
        public override Task CanGetNonExistentFileInfoAsync()
        {
            return base.CanGetNonExistentFileInfoAsync();
        }

        [Fact]
        public override Task CanSaveFilesAsync()
        {
            return base.CanSaveFilesAsync();
        }

        [Fact]
        public override Task CanManageFilesAsync()
        {
            return base.CanManageFilesAsync();
        }

        [Fact]
        public override Task CanRenameFilesAsync()
        {
            return base.CanRenameFilesAsync();
        }

        [Fact]
        public override void CanUseDataDirectory()
        {
            base.CanUseDataDirectory();
        }

        [Fact]
        public override Task CanDeleteEntireFolderAsync()
        {
            return base.CanDeleteEntireFolderAsync();
        }

        [Fact]
        public override Task CanDeleteEntireFolderWithWildcardAsync()
        {
            return base.CanDeleteEntireFolderWithWildcardAsync();
        }

        [Fact]
        public override Task CanDeleteSpecificFilesAsync()
        {
            return base.CanDeleteSpecificFilesAsync();
        }

        [Fact]
        public override Task CanDeleteNestedFolderAsync()
        {
            return base.CanDeleteNestedFolderAsync();
        }

        [Fact]
        public override Task CanDeleteSpecificFilesInNestedFolderAsync()
        {
            return base.CanDeleteSpecificFilesInNestedFolderAsync();
        }

        [Fact]
        public override Task CanRoundTripSeekableStreamAsync()
        {
            return base.CanRoundTripSeekableStreamAsync();
        }

        [Fact]
        public override Task WillRespectStreamOffsetAsync()
        {
            return base.WillRespectStreamOffsetAsync();
        }

        protected override IFileStorage GetStorage()
        {
            return new FileStorageScopedDecorator(
                new FolderFileStorage(o => o
                    .LoggerFactory(Substitute.For<ILoggerFactory>())
                    .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "tests_scoped"))),
                "scoped");
        }
    }
}
