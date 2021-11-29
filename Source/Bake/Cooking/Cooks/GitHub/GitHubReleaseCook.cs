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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes.GitHub;

namespace Bake.Cooking.Cooks.GitHub
{
    public class GitHubReleaseCook : Cook<GitHubReleaseRecipe>
    {
        private readonly IGitHub _gitHub;
        private readonly IFileSystem _fileSystem;

        public GitHubReleaseCook(
            IGitHub gitHub,
            IFileSystem fileSystem)
        {
            _gitHub = gitHub;
            _fileSystem = fileSystem;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            GitHubReleaseRecipe recipe,
            CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder()
                .AppendLine(recipe.ReleaseNotes.Notes)
                .AppendLine();

            var artifacts = await Task.WhenAll(recipe.Artifacts
                .OfType<FileArtifact>()
                .Select(async artifact =>
                {
                    var file = _fileSystem.Open(artifact.Path);
                    var sha256 = await file.GetHashAsync(
                        HashAlgorithm.SHA256,
                        cancellationToken);
                    return new
                        {
                            sha256,
                            file,
                            artifact
                        };
                }));
            foreach (var artifact in artifacts)
            {
                stringBuilder.AppendLine($"* `{artifact.artifact.Key.Name}`");
                stringBuilder.AppendLine($"  * SHA256: `{artifact.sha256}`");
            }

            var release = new Release(
                recipe.Version,
                recipe.Sha,
                artifacts.Select(a => a.file).ToArray(),
                stringBuilder.ToString());

            await _gitHub.CreateReleaseAsync(
                release,
                recipe.GitHubInformation,
                cancellationToken);

            return true;
        }
    }
}
