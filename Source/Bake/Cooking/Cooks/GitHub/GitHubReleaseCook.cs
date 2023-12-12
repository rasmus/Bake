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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes.GitHub;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Cooks.GitHub
{
    public class GitHubReleaseCook : Cook<GitHubReleaseRecipe>
    {
        private static readonly IReadOnlyDictionary<ExecutableOperatingSystem, string> NamingOs = new ConcurrentDictionary<ExecutableOperatingSystem, string>
            {
                [ExecutableOperatingSystem.Linux] = "linux",
                [ExecutableOperatingSystem.MacOSX] = "macosx",
                [ExecutableOperatingSystem.Windows] = "windows"
            };
        private static readonly IReadOnlyDictionary<ExecutableArchitecture, string> NamingArch = new ConcurrentDictionary<ExecutableArchitecture, string>
            {
                [ExecutableArchitecture.Intel32] = "x86",
                [ExecutableArchitecture.Intel64] = "x86_64",
            };

        private readonly ILogger<GitHubReleaseCook> _logger;
        private readonly IGitHub _gitHub;
        private readonly IFileSystem _fileSystem;

        public GitHubReleaseCook(
            ILogger<GitHubReleaseCook> logger,
            IGitHub gitHub,
            IFileSystem fileSystem)
        {
            _logger = logger;
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

            var stringBuilder = new StringBuilder();

            if (recipe.ReleaseNotes != null)
            {
                stringBuilder
                    .AppendLine("### Release notes")
                    .AppendLine(recipe.ReleaseNotes.Notes)
                    .AppendLine();
            }

            if (context.Ingredients.Changelog != null && context.Ingredients.Changelog.Any())
            {
                foreach (var commit in context.Ingredients.Changelog)
                {
                    stringBuilder.AppendLine($"* {commit.Message}");
                }

                stringBuilder.AppendLine();
            }

            var releaseFiles = (await CreateReleaseFilesAsync(additionalFiles, recipe, cancellationToken)).ToList();

            var documentationSite = recipe.Artifacts
                .OfType<DocumentationSiteArtifact>()
                .FirstOrDefault();
            if (documentationSite != null)
            {
                _logger.LogInformation("Documentation site built, packing it into a release file");
                var documentationZipFilePath = Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString("N"),
                    "documentation.zip");
                Directory.CreateDirectory(Path.GetDirectoryName(documentationZipFilePath));
                ZipFile.CreateFromDirectory(documentationSite.Path, documentationZipFilePath);
                var file = _fileSystem.Open(documentationZipFilePath);
                releaseFiles.Add(new ReleaseFile(
                    file,
                    $"documentation_v{context.Ingredients.Version}.zip",
                    await file.GetHashAsync(HashAlgorithm.SHA256, cancellationToken)));
            }

            var containerArtifacts = recipe.Artifacts
                .OfType<ContainerArtifact>()
                .ToArray();
            if (containerArtifacts.Any())
            {
                stringBuilder.AppendLine("### Containers");
                foreach (var containerArtifact in containerArtifacts)
                {
                    stringBuilder.AppendLine($"* `{containerArtifact.Name}`");
                    foreach (var tag in containerArtifact.Tags)
                    {
                        stringBuilder.AppendLine($"  * `{tag}`");
                    }
                }
            }

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
                .OfType<ExecutableArtifact>()
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



        private static string CalculateArtifactFileName(ExecutableArtifact artifact)
        {
            var parts = new[]
                {
                    artifact.Name,
                    NamingOs[artifact.Platform.Os],
                    NamingArch[artifact.Platform.Arch]
                };

            return $"{string.Join("_", parts)}.zip";
        }
    }
}
