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

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;
using Octokit;
using Release = Bake.ValueObjects.Release;

namespace Bake.Services
{
    public class GitHub : IGitHub
    {
        private readonly ILogger<GitHub> _logger;
        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHub(
            ILogger<GitHub> logger,
            IGitHubClientFactory gitHubClientFactory)
        {
            _logger = logger;
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task CreateReleaseAsync(
            Release release,
            GitHubInformation gitHubInformation,
            CancellationToken cancellationToken)
        {
            var gitHubClient = await _gitHubClientFactory.CreateAsync(
                string.Empty,
                cancellationToken);

            var tag = $"v{release.Version}";

            var gitHubRelease = await gitHubClient.Repository.Release.Create(
                gitHubInformation.Owner,
                gitHubInformation.Repository,
                new NewRelease(tag)
                {
                    Prerelease = release.Version.IsPrerelease,
                    TargetCommitish = release.Sha,
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

        private async Task UploadFileAsync(
            IFile file,
            Octokit.Release gitHubRelease,
            IGitHubClient gitHubClient,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug(
                "Uploading file {FileName} to GitHub release {ReleaseUrl}",
                file.FileName,
                gitHubRelease.Url);

            await using var stream = await file.OpenReadAsync(cancellationToken);

            await gitHubClient.Repository.Release.UploadAsset(
                gitHubRelease,
                new ReleaseAssetUpload
                {
                    ContentType = "application/octet-stream",
                    FileName = file.FileName,
                    RawData = stream,
                },
                cancellationToken);

            _logger.LogInformation(
                "Done uploading file {FileName} to GitHub release {ReleaseUrl} after {TotalSeconds} seconds",
                file.FileName,
                gitHubRelease.Url,
                stopwatch.Elapsed.TotalSeconds);
        }
    }
}
