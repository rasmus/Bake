// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
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
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.IntegrationTests.BakeTests
{
    public class Python3FlaskTests : BakeTest
    {
        public Python3FlaskTests() : base("Python3.Flask")
        {
        }

        [Test]
        public async Task Run()
        {
            // Arrange
            var version = SemVer.Random.ToString();
            var expectedImage = $"bake.local/python3-flask:{version}";

            // Act
            var returnCode = await ExecuteAsync(TestState.New(
                "run",
                "--convention=Release",
                "--build-version", version));

            // Assert
            returnCode.Should().Be(0);

            await AssertContainerPingsAsync(
                DockerArguments
                    .With(expectedImage)
                    .WithPort(5000));
        }
    }
}
