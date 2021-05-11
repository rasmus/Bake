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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task GatherAsync(ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            GitInformation gitInformation = null;

            await Task.Factory.StartNew(
                () => gitInformation = Gather(
                    ingredients.WorkingDirectory,
                    cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            ingredients.Git = gitInformation;
        }

        private GitInformation Gather(
            string workingDirectory,
            CancellationToken _)
        {
            static string Get(string d)
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

            workingDirectory = Get(workingDirectory);
            if (string.IsNullOrEmpty(workingDirectory))
            {
                _logger.LogWarning("No git repository found");
                return null;
            }

            using var repository = new Repository(workingDirectory);

            var originUrl = GetRemote(repository);
            var sha = repository.Head?.Tip?.Sha;

            return new GitInformation(
                sha,
                originUrl);
        }

        private Uri GetRemote(Repository repository)
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

            if (!Uri.TryCreate(remote.Url, UriKind.Absolute, out var url))
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




