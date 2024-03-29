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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using File = System.IO.File;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.IntegrationTests.BakeTests
{
    public class DockerFileSimpleTests : BakeTest
    {
        public DockerFileSimpleTests() : base("Dockerfile.Simple")
        {
        }

        [Test]
        public async Task Run()
        {
            // Arrange
            var version = SemVer.Random.ToString();
            var expectedContainerNameAndTag = $"awesome-container:{version}";

            // Act
            var returnCode = await ExecuteAsync(TestState.New(
                "run",
                "--convention=Release",
                "--destination=container>localhost:5000",
                "--build-version", version)
                .WithEnvironmentVariables(new Dictionary<string, string>
                    {
                        ["bake_credentials_docker_localhost_username"] = "registryuser",
                        ["bake_credentials_docker_localhost_password"] = "registrypassword",
                    }));

            // Assert
            returnCode.Should().Be(0);
            var images = await DockerHelper.ListImagesAsync();
            images.Should().Contain(new[]
            {
                $"bake.local/{expectedContainerNameAndTag}",
                $"localhost:5000/{expectedContainerNameAndTag}"
            });
        }

        [Test]
        public async Task NamedDockerfile()
        {
            // Arrange
            var version = SemVer.Random.ToString();
            var projectFilePath = Path.Combine(
                WorkingDirectory,
                "Awesome.Container",
                "bake.yaml");
            await File.WriteAllTextAsync(
                projectFilePath,
                "name: \"magic-container\"");
            var expectedContainerNameAndTag = $"magic-container:{version}";

            // Act
            var returnCode = await ExecuteAsync(TestState.New(
                    "run",
                    "--convention=Release",
                    "--destination=container>localhost:5000",
                    "--build-version", version)
                .WithEnvironmentVariables(new Dictionary<string, string>
                {
                    ["bake_credentials_docker_localhost_username"] = "registryuser",
                    ["bake_credentials_docker_localhost_password"] = "registrypassword",
                }));

            // Assert
            returnCode.Should().Be(0);
            var images = await DockerHelper.ListImagesAsync();
            images.Should().Contain(new []
            {
                $"bake.local/{expectedContainerNameAndTag}",
                $"localhost:5000/{expectedContainerNameAndTag}"
            });
        }
    }
}
