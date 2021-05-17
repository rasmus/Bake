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
            AssertFileExists(
                "bin", "Release", "netcoreapp3.1", "linux-x64", "publish", "NetCore.Console");
            AssertFileExists(
                "bin", "Release", "netcoreapp3.1", "win-x64", "publish", "NetCore.Console.exe");
        }

        [Test, Ignore("Cannot do this yet")]
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
