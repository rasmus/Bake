using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests
{
    public class NetCoreConsoleTests : TestProject
    {
        public NetCoreConsoleTests() : base("NetCore.Console")
        {
        }

        [TestCase("-h")]
        [TestCase("-?")]
        [TestCase("--help")]
        [TestCase("--version")]
        public async Task ValidArgs(params string[] args)
        {
            // Act
            var returnCode = await ExecuteAsync(args);

            // Assert
            returnCode.Should().Be(0);
        }
    }
}
