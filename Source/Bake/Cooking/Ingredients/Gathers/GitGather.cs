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
using Bake.ValueObjects;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class GitGather : IGather
    {
        private const string Origin = "origin";

        private readonly ILogger<GitGather> _logger;

        public GitGather(
            ILogger<GitGather> logger)
        {
            _logger = logger;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            try
            {
                await InternalGatherAsync(ingredients, cancellationToken);
            }
            catch (Exception e)
            {
                ingredients.FailGit();
                _logger.LogError(e, "Failed to gather git information");
            }
        }

        private async Task InternalGatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            GitInformation? gitInformation = null;

            await Task.Factory.StartNew(
                () => gitInformation = Gather(
                    ingredients.WorkingDirectory,
                    cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            if (gitInformation == null)
            {
                ingredients.FailGit();
            }
            else
            {
                ingredients.Git = gitInformation;
            }
        }

        private GitInformation? Gather(
            string workingDirectory,
            CancellationToken _)
        {
            static string? Get(string d)
            {
                while (true)
                {
                    if (Directory.Exists(Path.Combine(d, ".git")))
                    {
                        return d;
                    }

                    var parent = Directory.GetParent(d);
                    if (parent == null) return null;
                    d = parent.FullName;
                }
            }

            var projectRoot = Get(workingDirectory);
            if (string.IsNullOrEmpty(projectRoot))
            {
                _logger.LogWarning("No .git repository found");
                return null;
            }

            using var repository = new Repository(projectRoot);

            var originUrl = GetRemote(repository);
            var sha = repository.Head?.Tip?.Sha;
            var message = repository.Head?.Tip?.Message;

            if (originUrl == null ||
                string.IsNullOrEmpty(sha))
            {
                return null;
            }

            return new GitInformation(
                sha,
                originUrl,
                message ?? string.Empty);
        }

        private Uri? GetRemote(Repository repository)
        {
            var remote = repository.Network.Remotes.FirstOrDefault(
                r => string.Equals(r.Name, Origin, StringComparison.OrdinalIgnoreCase));
            if (remote == null)
            {
                _logger.LogWarning(
                    "No git remote named {RemoteName} found",
                    Origin);
                return null;
            }

            if (!GitRemoteParser.TryParse(remote.Url, out var url))
            {
                _logger.LogError(
                    "Could not transform URL {RemoteUrl} for remote {RemoteName}",
                    remote.Url,
                    Origin);
                return null;
            }

            return url;
        }
    }
}
