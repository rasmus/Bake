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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class ChangelogGather : IGather
    {
        private readonly ILogger<ChangelogGather> _logger;
        private readonly IGitHub _gitHub;
        private readonly IChangeLogBuilder _changeLogBuilder;

        public ChangelogGather(
            ILogger<ChangelogGather> logger,
            IGitHub gitHub,
            IChangeLogBuilder changeLogBuilder)
        {
            _logger = logger;
            _gitHub = gitHub;
            _changeLogBuilder = changeLogBuilder;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            GitInformation gitInformation;
            GitHubInformation gitHubInformation;

            try
            {
                gitInformation = await ingredients.GitTask;
                gitHubInformation = await ingredients.GitHubTask;
            }
            catch (OperationCanceledException)
            {
                ingredients.FailChangelog();
                return;
            }

            var tags = await _gitHub.GetTagsAsync(
                gitHubInformation,
                cancellationToken);

            if (tags.Count == 0)
            {
                _logger.LogInformation("Did not find any release tags");
                ingredients.FailChangelog();
                return;
            }

            var tag = tags
                .Where(t => t.Version.LegacyVersion < ingredients.Version.LegacyVersion)
                .MaxBy(t => t.Version);

            if (tag == null)
            {
                _logger.LogInformation("Could not find a su");
                ingredients.FailChangelog();
                return;
            }

            var pullRequests = await _gitHub.GetPullRequestsAsync(
                tag.Sha,
                gitInformation.Sha,
                gitHubInformation, cancellationToken);

            ingredients.Changelog = _changeLogBuilder.Build(pullRequests);
        }
    }
}
