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
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.Helpers
{
    public abstract class BakeTest : TestProject
    {
        private CancellationTokenSource? _timeout;

        private List<Release> _releases = null!;
        protected IReadOnlyCollection<Release> Releases => _releases;

        protected BakeTest(string projectName) : base(projectName)
        {
        }

        [SetUp]
        public void SetUpBakeTest()
        {
            _timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            _releases = new List<Release>();
        }

        [TearDown]
        public void TearDownBakeTest()
        {
            _timeout?.Dispose();
            _timeout = null;
        }

        protected Task<int> ExecuteAsync(
            params string[] args)
        {
            return ExecuteAsync(TestState.New(args));
        }

        protected Task<int> ExecuteAsync(
            TestState testState)
        {
            void Override(IServiceCollection s)
            {
                ReplaceService<IEnvironmentVariables>(s, new TestEnvironmentVariables(testState.EnvironmentVariables));
                ReplaceService<IGitHub>(s, new TestGitHub(_releases));
                testState.Overrides?.Invoke(s);
            }

            return Program.EntryAsync(
                testState.Arguments,
                Override,
                CancellationToken.None);
        }

        private static void ReplaceService<T>(IServiceCollection serviceCollection, T instance)
            where T : class
        {
            var serviceType = typeof(T);
            if (!serviceType.IsInterface)
            {
                throw new ArgumentException($"'{serviceType.PrettyPrint()}' is not an interface");
            }

            var serviceDescriptors = serviceCollection
                .Where(s => s.ServiceType == serviceType)
                .ToList();

            if (!serviceDescriptors.Any())
            {
                throw new ArgumentException($"There are no services of type '{serviceType.PrettyPrint()}'");
            }

            foreach (var serviceDescriptor in serviceDescriptors)
            {
                serviceCollection.Remove(serviceDescriptor);
            }

            serviceCollection.AddSingleton(instance);
        }

        private class TestGitHub : IGitHub
        {
            private readonly List<Release> _releases;

            public TestGitHub(
                List<Release> releases)
            {
                _releases = releases;
            }

            public Task CreateReleaseAsync(
                Release release, 
                GitHubInformation gitHubInformation,
                CancellationToken cancellationToken)
            {
                _releases.Add(release);
                return Task.CompletedTask;
            }

            public Task<PullRequestInformation?> GetPullRequestInformationAsync(GitInformation gitInformation,
                GitHubInformation gitHubInformation,
                CancellationToken cancellationToken)
            {
                return Task.FromResult<PullRequestInformation?>(null);
            }

            public Task<IReadOnlyCollection<PullRequest>> GetPullRequestsAsync(string baseSha,
                string headSha,
                GitHubInformation gitHubInformation,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<PullRequest?> GetPullRequestAsync(
                GitHubInformation gitHubInformation,
                int number,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<IReadOnlyCollection<Tag>> GetTagsAsync(
                GitHubInformation gitHubInformation,
                CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyCollection<Tag>>(Array.Empty<Tag>());
            }
        }
    }
}
