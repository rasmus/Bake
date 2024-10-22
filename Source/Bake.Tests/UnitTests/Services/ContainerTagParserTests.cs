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

using Bake.Services;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.UnitTests.Services
{
    public class ContainerTagParserTests : TestFor<ContainerTagParser>
    {
        [TestCase(
            "rasmus/debug:latest",
            "",
            "rasmus",
            "debug",
            "latest")]
        [TestCase(
            "registry",
            "",
            "",
            "registry",
            "latest")]
        [TestCase(
            "registry:2",
            "",
            "",
            "registry",
            "2")]
        [TestCase(
            "localhost:5000/path/image",
            "localhost:5000",
            "path",
            "image",
            "latest")]
        [TestCase(
            "127.0.0.1/path/image:2",
            "127.0.0.1",
            "path",
            "image",
            "2")]
        [TestCase(
            "localhost/path/image:2",
            "localhost",
            "path",
            "image",
            "2")]
        [TestCase(
            "localhost/image:2",
            "localhost",
            "",
            "image",
            "2")]
        [TestCase(
            "localhost/image:v1.2.3",
            "localhost",
            "",
            "image",
            "v1.2.3")]
        [TestCase(
            "containers.example.org/my-org/my-path/somecontainer:v1.2.3",
            "containers.example.org",
            "my-org/my-path",
            "somecontainer",
            "v1.2.3")]
        public void SuccessTryParse(
            string image,
            string expectedHostAndPort,
            string expectedPath,
            string expectedName,
            string expectedTag)
        {
            // Act
            Sut.TryParse(image, out var containerImage).Should().BeTrue();

            // Assert
            containerImage!.HostAndPort.Should().Be(expectedHostAndPort);
            containerImage.Path.Should().Be(expectedPath);
            containerImage.Name.Should().Be(expectedName);
            containerImage.Label.Should().Be(expectedTag);
        }
    }
}
