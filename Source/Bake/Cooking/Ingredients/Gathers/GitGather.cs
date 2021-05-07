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
