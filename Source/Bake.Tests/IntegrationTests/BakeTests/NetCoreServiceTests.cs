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

using Bake.Core;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.BakeTests
{
    public class NetCoreServiceTests : BakeTest
    {
        public NetCoreServiceTests()
            : base("NetCore.Service")
        {
        }

        [Test]
        public async Task Run()
        {
            // Arrange
            var version = SemVer.Random.ToString();
            var expectedImageVersioned = $"bake.local/awesome-net-core-service:{version}";
            var expectedImageLatest = "bake.local/awesome-net-core-service:latest";

            // Act
            var returnCode = await ExecuteAsync(
                "run",
                "--build-version", version,
                "--push-container-latest-tag=true");

            // Assert
            returnCode.Should().Be(0);
            var images = await DockerHelper.ListImagesAsync(Timeout);
            images.Should().Contain(new[] {expectedImageVersioned, expectedImageLatest});
            await AssertContainerPingsAsync(
                DockerArguments
                    .With(expectedImageVersioned)
                    .WithPort(5123)
                    .WithEnvironmentVariable("URLS", "http://0.0.0.0:5123")
                    .WithReadOnlyFilesystem());
        }
    }
}
