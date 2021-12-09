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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;
using Octokit;
using Author = Bake.ValueObjects.Author;
using Commit = Bake.ValueObjects.Commit;
using Release = Bake.ValueObjects.Release;

namespace Bake.Services
{
    public class GitHub : IGitHub
    {
        private static readonly IReadOnlyCollection<Commit> EmptyCommits = new Commit[] { };

        private readonly ILogger<GitHub> _logger;
        private readonly ICredentials _credentials;
        private readonly IGitHubClientFactory _gitHubClientFactory;

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
            var token = await _credentials.TryGetGitHubTokenAsync(
                gitHubInformation.Url,
                cancellationToken);

            var gitHubClient = await _gitHubClientFactory.CreateAsync(
                token,
                cancellationToken);

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

            if (release.Files.Any())
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

        public async Task<IReadOnlyCollection<Commit>> CompareAsync(
            string sha,
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var token = await _credentials.TryGetGitHubTokenAsync(
                gitHubInformation.Url,
                cancellationToken);

            var gitHubClient = await _gitHubClientFactory.CreateAsync(
                token,
                cancellationToken);

            var branches = await gitHubClient.Repository.Branch.GetAll(
                gitHubInformation.Owner,
                gitHubInformation.Repository);

            // TODO: Allow better selection
            var releaseBranch = branches.SingleOrDefault(b => string.Equals(b.Name, "release"));
            if (releaseBranch == null)
            {
                return EmptyCommits;
            }

            var compareResult = await gitHubClient.Repository.Commit.Compare(
                gitHubInformation.Owner,
                gitHubInformation.Repository,
                releaseBranch.Commit.Sha,
                sha);
            if (compareResult.AheadBy <= 0 || compareResult.BehindBy >= 0)
            {
                return EmptyCommits;
            }

            return compareResult.Commits
                .Select(c => new Commit(
                    c.Commit.Message,
                    c.Commit.Sha,
                    c.Commit.Author.Date,
                    new Author(
                        c.Commit.Author.Name,
                        c.Commit.Author.Email)))
                .ToList();
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
