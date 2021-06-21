// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.IO;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using Serilog.Events;

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
            // Arrange
            using var _ = SetEnvironmentVariable("bake_credentials_nuget_localhost_apikey", "acd0b30512ac4fa39f62eb7a61fcf56c");

            // Act
            var returnCode = await ExecuteAsync(
                "run",
                "--convention=Release",
                "--destination=nuget>http://localhost:5555/v3/index.json",
                "--build-version", SemVer.Random.ToString());

            // Assert
            returnCode.Should().Be(0);
            AssertSuccessfulArtifacts();
        }

        [TestCase(LogEventLevel.Verbose)]
        [TestCase(LogEventLevel.Debug)]
        [TestCase(LogEventLevel.Information)]
        [TestCase(LogEventLevel.Warning)]
        [TestCase(LogEventLevel.Error)]
        [TestCase(LogEventLevel.Fatal)]
        public async Task PlanThenApply(LogEventLevel logLevel)
        {
            // Act - plan
            var planPath = Path.Combine(WorkingDirectory, "plan.bake");
            var returnCode = await ExecuteAsync(
                "plan",
                "--destination=nuget>github",
                $"--log-level:{logLevel}",
                "--build-version", SemVer.Random.ToString(),
                "--plan-path", $"\"{planPath}\"");

            // Assert - plan
            returnCode.Should().Be(0);

            // Act - apply
            returnCode = await ExecuteAsync(
                "apply",
                "--plan-path", $"\"{planPath}\"");

            // Assert - apply
            returnCode.Should().Be(0);
            AssertSuccessfulArtifacts();
        }

        private void AssertSuccessfulArtifacts()
        {
            AssertFileExists(
                50L.MB(),
                "bin", "Release", "publish", "linux-x64", "NetCore.Console");
            AssertFileExists(
                50L.MB(),
                "bin", "Release", "publish", "win-x64", "NetCore.Console.exe");
        }

        [TestCase("-h")]
        [TestCase("-?")]
        [TestCase("--help")]
        [TestCase("--version")]
        [TestCase("-v")]
        [TestCase("run", "--help")]
        [TestCase("apply", "--help")]
        [TestCase("plan", "--help")]
        public async Task BasicArgsTest(params string[] args)
        {
            // Act
            var returnCode = await ExecuteAsync(args);

            // Assert
            returnCode.Should().Be(0);
        }
    }
}
