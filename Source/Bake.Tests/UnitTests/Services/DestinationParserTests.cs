// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects.Destinations;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.UnitTests.Services
{
    public class DestinationParserTests : TestFor<DestinationParser>
    {
        [SetUp]
        public void SetUp()
        {
            Inject<IDefaults>(new Defaults(TestEnvironmentVariables.None));
        }

        [TestCase(
            "nuget",
            "https://api.nuget.org/v3/index.json")]
        [TestCase(
            "nuget>http://localhost:5555/v3/index.json",
            "http://localhost:5555/v3/index.json")]
        public void NuGetRegistry(
            string input,
            string expectedRegistryUrl)
        {
            // Act
            Sut.TryParse(input, out var destination).Should().BeTrue();

            // Assert
            ((NuGetRegistryDestination) destination).Url.AbsoluteUri.Should().Be(expectedRegistryUrl);
        }

        [TestCase(
            "container>rasmus",
            "rasmus/")]
        [TestCase(
            "container>localhost:5000",
            "localhost:5000/")]
        [TestCase(
            "container>localhost:5000/",
            "localhost:5000/")]
        public void DockerRegistry(
            string input,
            string expectedRegistryUrl)
        {
            // Act
            Sut.TryParse(input, out var destination).Should().BeTrue();

            // Assert
            ((ContainerRegistryDestination)destination).Url.Should().Be(expectedRegistryUrl);
        }

        [TestCase(
            "helm-chart>https://localhost:4321/",
            "https://localhost:4321/")]
        public void HelmChart(
            string input,
            string expectedRegistryUrl)
        {
            // Act
            Sut.TryParse(input, out var destination).Should().BeTrue();

            // Assert
            ((OctopusDeployDestination)destination).Url.Should().Be(expectedRegistryUrl);
        }

        [TestCase(
            "container>github",
            "container",
            "github")]
        [TestCase(
            "nuget>github",
            "nuget",
            "github")]
        [TestCase(
            "release>github",
            "release",
            "github")]
        public void Dynamics(
            string input,
            string expectedArtifactType,
            string expectedDestination)
        {
            // Act
            Sut.TryParse(input, out var destination).Should().BeTrue();

            // Assert
            var dynamicDestination = (DynamicDestination)destination;
            dynamicDestination.ArtifactType.Should().Be(expectedArtifactType);
            dynamicDestination.Destination.Should().Be(expectedDestination);
        }
    }
}
