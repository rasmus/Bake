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
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Core
{
    public class CredentialsTests : TestFor<Credentials>
    {
        [TestCase(
            "http://localhost:5555/v3/index.json",
            "bake_credentials_nuget_localhost_apikey", "acd0b30512ac4fa39f62eb7a61fcf56c")]
        public async Task GetNuGetApiKeyAsync(
            string url,
            string environmentKey,
            string expectedCredentials)
        {
            // Arrange
            Inject<IEnvironmentVariables>(new TestEnvironmentVariables(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [environmentKey] = expectedCredentials
                }));

            // Act
            var credentials = await Sut.GetNuGetApiKeyAsync(
                new Uri(url),
                CancellationToken.None);

            // Assert
            credentials.Should().Be(expectedCredentials);
        }
    }
}
