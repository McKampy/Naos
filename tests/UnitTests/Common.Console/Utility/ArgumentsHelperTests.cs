﻿namespace Naos.Core.UnitTests.Common.Console
{
    using Naos.Core.Common.Console;
    using Shouldly;
    using Xunit;

    public class ArgumentsHelperTests
    {
        [Fact]
        public void CanParseLine_Test()
        {
            var args = ArgumentsHelper.Split("git commit -m 'message 123' ");

            args.ShouldContain("git");
            args.ShouldContain("commit");
            args.ShouldContain("-m");
            args.ShouldContain("message 123");
            args.ShouldNotContain(" ");
            args.ShouldNotContain(string.Empty);
        }
    }
}
