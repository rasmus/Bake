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
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using NSubstitute;
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
            var credentials = Substitute.For<ICredentials>();
            credentials
                .TryGetGitHubTokenAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(testToken!));

            Inject(credentials);
            Inject<IGitHubClientFactory>(new GitHubClientFactory());
        }

        [Test]
        public async Task GetPullRequestInformationAsync()
        {
            // Act
            var pullRequestInformation = await Sut.GetPullRequestInformationAsync(
                new GitInformation(
                    "5cb8fb6fae922909bd8a44f7d029dcda377facc7",
                    new Uri("https://github.com/rasmus/Bake"),
                    "Merge 679f3e6 into 6ddb502"),
                new GitHubInformation(
                    "rasmus",
                    "Bake",
                    new Uri("https://github.com/rasmus/Bake"),
                    new Uri("https://api.github.com/")),
                CancellationToken.None);

            // Assert
            pullRequestInformation.Should().NotBeNull();
            pullRequestInformation!.Labels.Should().NotBeEmpty();
        }
    }
}
