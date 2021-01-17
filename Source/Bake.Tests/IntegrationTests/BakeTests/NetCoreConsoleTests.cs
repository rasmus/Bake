using System.Threading.Tasks;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.BakeTests
{
    public class NetCoreConsoleTests : BakeTest
    {
        public NetCoreConsoleTests() : base("NetCore.Console")
        {
        }

        [Test]
        public async Task Run()
        {
            // Act
            var returnCode = await ExecuteAsync(
                "run",
                "--retry", "42");

            // Assert
            returnCode.Should().Be(0);
        }

        [TestCase("-h")]
        [TestCase("-?")]
        [TestCase("--help")]
        [TestCase("--version")]
        public async Task BasicArgsTest(params string[] args)
        {
            // Act
            var returnCode = await ExecuteAsync(args);

            // Assert
            returnCode.Should().Be(0);
        }
    }
}
