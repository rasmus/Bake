// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.IntegrationTests.BakeTests
{
    public class GoLangServiceTests : BakeTest
    {
        public GoLangServiceTests() : base("GoLang.Service")
        {
        }

        [Test]
        public async Task Run()
        {
            // Arrange
            var version = SemVer.Random.ToString();
            var expectedImage = $"bake.local/golang-service:{version}";

            // Act
            var returnCode = await ExecuteAsync(TestState.New(
                "run",
                "--build-version", version));

            // Assert
            returnCode.Should().Be(0);
            AssertFileExists(
                1L.MB(),
                "linux-x64",
                "golang-service");
            AssertFileExists(
                1L.MB(),
                "osx-x64",
                "golang-service");
            AssertFileExists(
                1L.MB(),
                "win-x64",
                "golang-service.exe");

            await AssertContainerPingsAsync(
                DockerArguments
                    .With(expectedImage)
                    .WithPort(8080));
        }
    }
}
