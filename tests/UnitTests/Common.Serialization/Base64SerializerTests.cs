﻿namespace Naos.Core.UnitTests.Common.Serialization
{
    using Naos.Core.Common.Serialization;
    using Xunit;

    public class Base64SerializerTests : SerializerTestsBase
    {
        [Fact]
        public override void CanRoundTripBytes()
        {
            base.CanRoundTripBytes();
        }

        [Fact(Skip = "string not working")]
        public override void CanRoundTripString()
        {
            base.CanRoundTripString();
        }

        [Fact(Skip = "Skip benchmarks for now")]
        public virtual void Benchmark()
        {
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<Base64SerializerBenchmark>();
        }

        protected override ISerializer GetSerializer()
        {
            return new Base64Serializer();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class Base64SerializerBenchmark : SerializerBenchmarkBase
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected override ISerializer GetSerializer()
        {
            return new Base64Serializer();
        }
    }
}
