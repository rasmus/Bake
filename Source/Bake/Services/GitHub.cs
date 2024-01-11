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

using System.Diagnostics;
using System.Text.RegularExpressions;
using Bake.Core;
using Bake.Exceptions;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;
using Octokit;
using Author = Bake.ValueObjects.Author;
using Commit = Bake.ValueObjects.Commit;
using PullRequest = Bake.ValueObjects.PullRequest;
using Release = Bake.ValueObjects.Release;

namespace Bake.Services
{
    public class GitHub : IGitHub
    {
        private readonly ILogger<GitHub> _logger;
        private readonly ICredentials _credentials;
        private readonly IGitHubClientFactory _gitHubClientFactory;
        public static readonly Regex SpecialMergeCommitMessageParser = new(
            @"Merge\s+(?<pr>[a-f0-9]+)\s+into\s+(?<base>[a-f0-9]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex IsMergeCommit = new(
            @"^Merge\s+pull\s+request\s+\#(?<pr>[0-9]+)\s+.*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public GitHub(
            ILogger<GitHub> logger,
            ICredentials credentials,
            IGitHubClientFactory gitHubClientFactory)
        {
            _logger = logger;
            _credentials = credentials;
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task CreateReleaseAsync(
            Release release,
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var gitHubClient = await CreateGitHubClientAsync(gitHubInformation, cancellationToken);
            if (gitHubClient == null)
            {
                throw new BuildFailedException(
                    "Could not create a GitHub release due to missing credentials");
            }

            var tag = $"v{release.Version}";

            var gitHubRelease = await gitHubClient.Repository.Release.Create(
                gitHubInformation.Owner,
                gitHubInformation.Repository,
                new NewRelease(tag)
                {
                    Prerelease = release.Version.IsPrerelease,
                    TargetCommitish = release.Sha,
                    Body = release.Body,
                    Draft = true,
                    Name = $"v{release.Version}",
                });

            if (release.Files.Count == 0)
            {
                var uploadTasks = release.Files
                    .Select(f => UploadFileAsync(f, gitHubRelease, gitHubClient, cancellationToken));
                await Task.WhenAll(uploadTasks);
            }

            var gitHubReleaseUpdate = gitHubRelease.ToUpdate();
            gitHubReleaseUpdate.Draft = false;
            await gitHubClient.Repository.Release.Edit(
                gitHubInformation.Owner,
                gitHubInformation.Repository,
                gitHubRelease.Id,
                gitHubReleaseUpdate);
        }

        public async Task<IReadOnlyCollection<Tag>> GetTagsAsync(
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var gitHubClient = await CreateGitHubClientAsync(gitHubInformation, cancellationToken);
            if (gitHubClient == null)
            {
                return Array.Empty<Tag>();
            }

            var releases = await gitHubClient.Repository.GetAllTags(
                gitHubInformation.Owner,
                gitHubInformation.Repository);

            return releases
                .Select(t => new
                {
                    version = SemVer.TryParse(t.Name, out var v, true) ? v : null,
                    tag = t,
                })
                .Where(a => !ReferenceEquals(a.version, null))
                .Select(a => new Tag(a.version!, a.tag.Commit.Sha))
                .ToArray();
        }

        public async Task<PullRequestInformation?> GetPullRequestInformationAsync(
            GitInformation gitInformation,
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var gitHubClient = await CreateGitHubClientAsync(gitHubInformation, cancellationToken);
            if (gitHubClient == null)
            {
                return null;
            }

            var pullRequestInformation = await SearchAsync(gitInformation.Sha);
            if (pullRequestInformation != null)
            {
                return pullRequestInformation;
            }

            if (string.IsNullOrEmpty(gitInformation.Message))
            {
                return null;
            }

            var match = SpecialMergeCommitMessageParser.Match(gitInformation.Message);
            if (!match.Success)
            {
                return null;
            }

            _logger.LogDebug(
                "This commit {PreMergeCommitSha} looks like a special GitHub pre-merge commit for the actual commit {CommitSha}",
                match.Groups["pr"].Value, match.Groups["base"].Value);

            return await SearchAsync(match.Groups["pr"].Value);

            async Task<PullRequestInformation?> SearchAsync(string c)
            {
                var issues = await gitHubClient.Search.SearchIssues(new SearchIssuesRequest(c)
                {
                    Type = IssueTypeQualifier.PullRequest,
                    Repos = new RepositoryCollection
                    {
                        {gitHubInformation.Owner, gitHubInformation.Repository},
                    }
                });
                if (issues.Items.Count != 1)
                {
                    return null;
                }

                var issue = issues.Items.Single();
                return new PullRequestInformation(
                    issue.Labels.Select(l => l.Name).ToArray());
            }
        }

        private async Task<IGitHubClient?> CreateGitHubClientAsync(
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var token = await _credentials.TryGetGitHubTokenAsync(
                gitHubInformation.Url,
                cancellationToken);

            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var gitHubClient = await _gitHubClientFactory.CreateAsync(
                token,
                gitHubInformation.ApiUrl,
                cancellationToken);

            return gitHubClient;
        }

        public async Task<IReadOnlyCollection<Commit>> GetCommitsAsync(
            string baseSha,
            string headSha,
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var gitHubClient = await CreateGitHubClientAsync(
                gitHubInformation,
                cancellationToken);

            if (gitHubClient == null)
            {
                return Array.Empty<Commit>();
            }

            var compareResult = await gitHubClient.Repository.Commit.Compare(
                gitHubInformation.Owner,
                gitHubInformation.Repository,
                baseSha,
                headSha);
            if (compareResult.AheadBy <= 0)
            {
                return Array.Empty<Commit>();
            }

            return compareResult.Commits
                .Select(c => new Commit(
                    c.Commit.Message,
                    c.Sha,
                    c.Commit.Author.Date,
                    new Author(
                        c.Commit.Author.Name,
                        c.Commit.Author.Email)))
                .ToList();
        }

        public async Task<IReadOnlyCollection<PullRequest>> GetPullRequestsAsync(
            string baseSha,
            string headSha,
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var commits = await GetCommitsAsync(
                baseSha,
                headSha,
                gitHubInformation,
                cancellationToken);

            if (commits.Count == 0)
            {
                return Array.Empty<PullRequest>();
            }

            var pullRequestTasks = commits
                .Select(c => IsMergeCommit.Match(c.Message))
                .Where(m => m.Success)
                .Select(m => GetPullRequestAsync(gitHubInformation, int.Parse(m.Groups["pr"].Value), cancellationToken));

            var pullRequests = await Task.WhenAll(pullRequestTasks);

            return pullRequests
                .Where(pr => pr != null)
                .ToArray()!;
        }

        public async Task<PullRequest?> GetPullRequestAsync(
            GitHubInformation gitHubInformation,
            int number,
            CancellationToken cancellationToken)
        {
            var token = await _credentials.TryGetGitHubTokenAsync(
                gitHubInformation.Url,
                cancellationToken);

            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var gitHubClient = await _gitHubClientFactory.CreateAsync(
                token,
                gitHubInformation.ApiUrl,
                cancellationToken);

            try
            {
                var pullRequest = await gitHubClient.PullRequest.Get(
                    gitHubInformation.Owner,
                    gitHubInformation.Repository,
                    number);

                var author = pullRequest.User.Login;
                var i = author.IndexOf('[');
                var length = i < 0 ? author.Length : i;

                return new PullRequest(
                    pullRequest.Number,
                    pullRequest.Title,
                    new []
                    {
                        author[..length],
                    });
            }
            catch (NotFoundException e)
            {
                // Puke! (why throw exception for something that is likely to happen)

                _logger.LogDebug(
                    e, "Could not find pull request #{Number} in {Owner}/{Repository}",
                    number,
                    gitHubInformation.Owner,
                    gitHubInformation.Repository);
                return null;
            }
        }

        private async Task UploadFileAsync(
            ReleaseFile releaseFile,
            Octokit.Release gitHubRelease,
            IGitHubClient gitHubClient,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug(
                "Uploading releaseFile {FileName} to GitHub release {ReleaseUrl}",
                releaseFile.Source.FileName,
                gitHubRelease.Url);

            await using var stream = await releaseFile.Source.OpenReadAsync(cancellationToken);

            try
            {
                await gitHubClient.Repository.Release.UploadAsset(
                    gitHubRelease,
                    new ReleaseAssetUpload
                    {
                        ContentType = "application/octet-stream",
                        FileName = releaseFile.Destination,
                        RawData = stream,
                    },
                    cancellationToken);
            }
            catch (ApiValidationException e)
            {
                _logger.LogCritical(e, "Upload to GitHub failed: {GitHibMessage}", e.Message);
                throw;
            }

            _logger.LogInformation(
                "Done uploading releaseFile {FileName} to GitHub release {ReleaseUrl} after {TotalSeconds} seconds",
                releaseFile.Source.FileName,
                gitHubRelease.Url,
                stopwatch.Elapsed.TotalSeconds);
        }
    }
}
