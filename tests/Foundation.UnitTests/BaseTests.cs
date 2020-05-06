﻿namespace Naos.Foundation.UnitTests
{
    using System;
    using System.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Xunit.Abstractions;

    public abstract class BaseTests
    {
        private static IConfigurationRoot configuration;

        static BaseTests()
        {
            Environment.SetEnvironmentVariable(EnvironmentKeys.Environment, "Development");
            Environment.SetEnvironmentVariable(EnvironmentKeys.IsLocal, "True");
        }

        protected static IConfiguration Configuration => configuration ??= NaosConfigurationFactory.Create();

        protected long Benchmark(Action action, int iterations = 1, ITestOutputHelper output = null)
        {
            GC.Collect();
            output?.WriteLine($"Execution with #{iterations} iterations started: \r\n  - Gen-0: {GC.CollectionCount(0)}, Gen-1: {GC.CollectionCount(1)}, Gen-2: {GC.CollectionCount(2)}");
            var sw = new Stopwatch();
            action(); // trigger jit before execution

            sw.Start();
            for (var i = 1; i <= iterations; i++)
            {
                action();
            }

            sw.Stop();
            GC.Collect();
            output?.WriteLine($"Execution with #{iterations} iterations took: {sw.Elapsed.TotalMilliseconds}ms\r\n  - Gen-0: {GC.CollectionCount(0)}, Gen-1: {GC.CollectionCount(1)}, Gen-2: {GC.CollectionCount(2)}", sw.ElapsedMilliseconds);

            return sw.ElapsedMilliseconds;
        }
    }
}