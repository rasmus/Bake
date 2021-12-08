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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Cooks.GitHub;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes.GitHub;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Bake.Tests.ExplicitTests
{
    [Explicit]
    public class GitHubReleaseCookTests : ServiceTest<GitHubReleaseCook>
    {
        public GitHubReleaseCookTests() : base("Dockerfile.Simple") { }

        [Test]
        public async Task CreateRelease()
        {
            // Arrange
            var recipe = new GitHubReleaseRecipe(
                new GitHubInformation(
                    "rasmus",
                    "testtest",
                    new Uri("https://github.com/rasmus/testtest")),
                SemVer.Random,
                "a108d8a38b4ac154172cb7eeea8530e316ead798",
                new ReleaseNotes(SemVer.Random, "This is a test"),
                new Artifact[]
                {
                    new ExecutableFileArtifact(
                        new ArtifactKey(ArtifactType.Executable, "test_linux"),
                        Path.Combine(WorkingDirectory, "README.md"),
                        ExecutableOperatingSystem.Linux,
                        ExecutableArchitecture.Intel64)
                });

            // Arrange
            var result = await Sut.CookAsync(
                null,
                recipe,
                CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        private static string GetToken()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<GitHubReleaseCookTests>()
                .Build()["github.token"];
        }

        protected override IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return base.Configure(serviceCollection)
                .AddSingleton<IGitHub, GitHub>()
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
