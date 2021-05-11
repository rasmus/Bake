using System;
using System.IO;
using System.Threading.Tasks;
using Bake.Core;
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
                "--build-version", SemVer.Random.ToString());

            // Assert
            returnCode.Should().Be(0);
        }

        [Test, Ignore("Deserialize does not work yet")]
        public async Task PlanThenApply()
        {
            // Act - plan
            var planPath = Path.Combine(WorkingDirectory, "plan.bake");
            var returnCode = await ExecuteAsync(
                "plan",
                "--build-version", "1.2.3",
                "--plan-path", $"\"{planPath}\"");

            // Assert - plan
            returnCode.Should().Be(0);
            Console.WriteLine(await File.ReadAllTextAsync(planPath));

            // Act - apply
            returnCode = await ExecuteAsync(
                "apply",
                "--plan-path", $"\"{planPath}\"");

            // Assert - apply
            returnCode.Should().Be(0);
        }

        [TestCase("-h")]
        [TestCase("-?")]
        [TestCase("--help")]
        [TestCase("--version")]
        [TestCase("-v")]
        public async Task BasicArgsTest(params string[] args)
        {
            // Act
            var returnCode = await ExecuteAsync(args);

            // Assert
            returnCode.Should().Be(0);
        }
    }
}
