﻿namespace Naos.Core.UnitTests.Common.Serialization
{
    using Naos.Core.Common;
    using Xunit;

    public class MessagePackSerializerTests : SerializerTestsBase
    {
        [Fact]
        public override void CanRoundTripBytes()
        {
            base.CanRoundTripBytes();
        }

        [Fact]
        public override void CanRoundTripString()
        {
            base.CanRoundTripString();
        }

        [Fact(Skip = "Skip benchmarks for now")]
        public virtual void Benchmark()
        {
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<MessagePackSerializerBenchmark>();
        }

        protected override ISerializer GetSerializer()
        {
            return new MessagePackSerializer();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class MessagePackSerializerBenchmark : SerializerBenchmarkBase
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected override ISerializer GetSerializer()
        {
            return new MessagePackSerializer();
        }
    }
}