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
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
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
                "--print-plan=true",
                "--build-version", version));

            // Assert
            returnCode.Should().Be(0);
            AssertFileExists(
                1L.MB(),
                "golang-service");
            AssertFileExists(
                1L.MB(),
                "golang-service.exe");

            using var _ = await DockerHelper.RunAsync(
                expectedImage,
                new Dictionary<int, int> {[8080] = 8080},
                CancellationToken.None);
            using var httpClient = new HttpClient();
            var start = Stopwatch.StartNew();
            var success = false;
            while (start.Elapsed < TimeSpan.FromSeconds(30))
            {
                try
                {
                    using var response = await httpClient.GetAsync("http://localhost:8080/ping");
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Status code {response.StatusCode}");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Got content: {Environment.NewLine}{content}");

                    success = true;
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed with {e.GetType().Name}: {e.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            success.Should().BeTrue();
        }
    }
}
