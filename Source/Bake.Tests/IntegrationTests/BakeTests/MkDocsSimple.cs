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

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.IntegrationTests.BakeTests
{
    public class MkDocsSimple : BakeTest
    {
        public MkDocsSimple() : base("MkDocs.Simple")
        {
        }

        [Test]
        public async Task Run()
        {
            // Arrange
            var version = SemVer.Random.ToString();
            var expectedImage = $"bake.local/docs:{version}";

            // Act
            var returnCode = await ExecuteAsync(TestState.New(
                "run",
                "--convention=Release",
                "--destination=release>github,documentation-site>container",
                "--build-version", version));

            // Assert
            returnCode.Should().Be(0);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // thttpd is very picky about file permissions, which makes the container
                // impossible to test on Windows :(
                // http://acme.com/software/thttpd/thttpd_man.html#PERMISSIONS

                await AssertContainerPingsAsync(
                    DockerArguments.With(expectedImage)
                        .WithPort(8080),
                    "index.html");
            }
        }
    }
}
