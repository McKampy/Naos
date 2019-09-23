﻿namespace Naos.UnitTests.FileStorage.Infrastructure
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.FileStorage.Domain;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using NSubstitute;
    using Xunit;

    public class EmbeddedFileStorageTests
    {
        [Fact]
        public async Task CanGetFileInfoAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(@"Naos\UnitTests/FileStorage\StubFile.txt").AnyContext();
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.ContentType == ContentType.TEXT);
            }
        }

        [Fact]
        public async Task CanGetJsonFileInfoAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(@"Naos\UnitTests/FileStorage\StubFile.json").AnyContext();
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.ContentType == ContentType.JSON);
            }
        }

        [Fact]
        public async Task CaHandleNonExistingFileInfoAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(@"Naos\UnitTests\FileStorage\DoesNotExist.txt").AnyContext();
                Assert.Null(fileInfo);
            }
        }

        [Fact]
        public async Task CanGetFileInfosAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var fileInfos = await storage.GetFileInformationsAsync().AnyContext();
                Assert.NotNull(fileInfos);
                Assert.NotNull(fileInfos.Files);
                Assert.True(fileInfos.Files.Any());
            }
        }

        [Fact]
        public async Task CanGetFileInfosByPatternAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var fileInfos = await storage.GetFileInformationsAsync(searchPattern: "*StubFile.*").AnyContext();
                Assert.NotNull(fileInfos);
                Assert.NotNull(fileInfos.Files);
                Assert.True(fileInfos.Files.Any());
            }
        }

        [Fact]
        public async Task CanGetFileStreamAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var stream = await storage.GetFileStreamAsync(@"Naos\UnitTests\FileStorage\StubFile.txt").AnyContext();
                Assert.NotNull(stream);
                Assert.True(stream.Length > 0);
            }
        }

        [Fact]
        public async Task CanHandleNonExistingFileStreamAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var stream = await storage.GetFileStreamAsync(@"Naos\UnitTests\FileStorage\DoesNotExist.txt").AnyContext();
                Assert.Null(stream);
            }
        }

        protected IFileStorage GetStorage()
        {
            return new EmbeddedFileStorage(o => o
                .LoggerFactory(Substitute.For<ILoggerFactory>())
                .Assembly(new[] { Assembly.GetExecutingAssembly() }));
        }
    }
}
