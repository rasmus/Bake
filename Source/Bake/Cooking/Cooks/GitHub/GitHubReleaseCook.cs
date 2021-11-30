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
            var additionalFiles = new[]
                {
                    Path.Combine(context.Ingredients.WorkingDirectory, "README.md"),
                    Path.Combine(context.Ingredients.WorkingDirectory, "LICENSE"),
                    Path.Combine(context.Ingredients.WorkingDirectory, "RELEASE_NOTES.md"),
                }
                .Where(System.IO.File.Exists)
                .Select(p => _fileSystem.Open(p))
                .ToArray();

            var stringBuilder = new StringBuilder()
                .AppendLine("### Release notes")
                .AppendLine(recipe.ReleaseNotes.Notes)
                .AppendLine();

            var releaseFiles = await CreateReleaseFilesAsync(additionalFiles, recipe, cancellationToken);
            if (releaseFiles.Any())
            {
                stringBuilder.AppendLine("### Files");
                foreach (var releaseFile in releaseFiles)
                {
                    stringBuilder.AppendLine($"* `{releaseFile.Destination}`");
                    stringBuilder.AppendLine($"  * SHA256: `{releaseFile.Sha256}`");
                }
            }

            var release = new Release(
                recipe.Version,
                recipe.Sha,
                stringBuilder.ToString(),
                releaseFiles);

            await _gitHub.CreateReleaseAsync(
                release,
                recipe.GitHubInformation,
                cancellationToken);

            return true;
        }

        private async Task<IReadOnlyCollection<ReleaseFile>> CreateReleaseFilesAsync(
            IReadOnlyCollection<IFile> additionalFiles,
            GitHubReleaseRecipe recipe,
            CancellationToken cancellationToken)
        {
            return await Task.WhenAll(recipe.Artifacts
                .OfType<FileArtifact>()
                .Select(async artifact =>
                {
                    var file = _fileSystem.Open(artifact.Path);
                    var fileName = CalculateArtifactFileName(artifact);
                    var compressedFile = await _fileSystem.CompressAsync(
                        fileName,
                        CompressionAlgorithm.ZIP,
                        Enumerable.Empty<IFile>()
                            .Concat(additionalFiles)
                            .Concat(new[] {file,})
                            .ToArray(),
                        cancellationToken);
                    var sha256 = await compressedFile.GetHashAsync(
                        HashAlgorithm.SHA256,
                        cancellationToken);
                    return new ReleaseFile(
                        compressedFile,
                        fileName,
                        sha256);
                }));
        }

        private static string CalculateArtifactFileName(FileArtifact artifact)
        {
            return artifact.Key.Type switch
                {
                    ArtifactType.ToolLinux => $"{artifact.Key.Name}_linux_amd64.zip",
                    ArtifactType.ToolWindows => $"{artifact.Key.Name}_windows_amd64.zip",
                    _ => throw new ArgumentOutOfRangeException()
                };
        }
    }
}
