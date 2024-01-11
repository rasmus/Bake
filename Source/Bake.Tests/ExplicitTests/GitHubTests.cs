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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Cooks.GitHub;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Bake.Tests.ExplicitTests
{
    [Explicit]
    public class GitHubTests : ServiceTest<GitHub>
    {
        public GitHubTests() : base(null) { }

        [Test]
        public async Task GetCommits()
        {
            var commits = await Sut.GetCommitsAsync(
                "e1b486d",
                "c54a03b",
                new GitHubInformation(
                    "rasmus",
                    "Bake",
                    new Uri("https://github.com/rasmus/Bake"),
                    new Uri("https://api.github.com/")),
                CancellationToken.None);
        }

        [Test]
        public async Task GetPullRequests()
        {
            var pullRequests = await Sut.GetPullRequestsAsync(
                "e1b486d",
                "c54a03b",
                new GitHubInformation(
                    "rasmus",
                    "Bake",
                    new Uri("https://github.com/rasmus/Bake"),
                    new Uri("https://api.github.com/")),
                CancellationToken.None);

            foreach (var pullRequest in pullRequests)
            {
                Console.WriteLine($"new ({pullRequest.Number}, \"{pullRequest.Title}\", new []{{\"{pullRequest.Authors.Single()}\"}}),");
            }
        }

        [Test]
        public async Task GetPullRequest_NonExisting()
        {
            // Act
            var pullRequest = await Sut.GetPullRequestAsync(
                new GitHubInformation(
                    "rasmus",
                    "Bake",
                    new Uri("https://github.com/rasmus/Bake"),
                    new Uri("https://api.github.com/")),
                int.MaxValue,
                CancellationToken.None);

            // Assert
            pullRequest.Should().BeNull();
        }

        private static string GetToken()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<GitHubReleaseCookTests>()
                .Build()["github:token"]!;
        }

        protected override IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return base.Configure(serviceCollection)
                .AddSingleton<GitHub>()
                .AddSingleton<IGitHubClientFactory, GitHubClientFactory>()
                .AddSingleton<ICredentials, Credentials>()
                .AddSingleton<GitHubReleaseCook>()
                .AddSingleton<IDefaults, Defaults>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IEnvironmentVariables>(new TestEnvironmentVariables(new Dictionary<string, string>
                {
                    ["github_personal_token"] = GetToken(),
                }));
        }
    }
}
