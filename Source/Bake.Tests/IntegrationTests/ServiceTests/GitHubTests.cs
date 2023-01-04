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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.ServiceTests
{
    public class GitHubTests : TestFor<GitHub>
    {
        [SetUp]
        public void SetUp()
        {
            var testToken = new[]
                {
                    Environment.GetEnvironmentVariable("GITHUB_TOKEN"),
                    Environment.GetEnvironmentVariable("GITHUB_REPO_TOKEN")
                }
                .FirstOrDefault(t => !string.IsNullOrEmpty(t));

            testToken.Should().NotBeNullOrEmpty();
            var credentials = new Mock<ICredentials>();
            credentials
                .Setup(m => m.TryGetGitHubTokenAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testToken);

            Inject(credentials.Object);
            Inject<IGitHubClientFactory>(new GitHubClientFactory());
        }

        [Test]
        public async Task T()
        {
            // Act
            var pullRequestInformation = await Sut.GetPullRequestInformationAsync(
                "28fe9fb7238476731998d1fc2456cef2c417f2e7",
                new GitHubInformation(
                    "rasmus",
                    "Bake",
                    new Uri("https://github.com/rasmus/Bake"),
                    new Uri("https://api.github.com/")),
                CancellationToken.None);

            // Assert
            pullRequestInformation.Should().NotBeNull();
            pullRequestInformation.Labels.Should().NotBeEmpty();
        }
    }
}
