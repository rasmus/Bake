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

using System.Collections.Concurrent;
using System.Text;
using Bake.Core;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Release;
using Bake.ValueObjects.Releases;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace Bake.Cooking.Composers
{
    public class ReleaseComposer : Composer
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

        public override IReadOnlyCollection<ArtifactType> Consumes { get; } =
        [
            ArtifactType.Container,
            ArtifactType.DocumentationSite,
            ArtifactType.Executable,
            ArtifactType.HelmChart,
            ArtifactType.NuGet,
        ];

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = [ArtifactType.Release];

        private readonly ILogger<ReleaseComposer> _logger;
        private readonly IDefaults _defaults;

        public ReleaseComposer(
            ILogger<ReleaseComposer> logger,
            IDefaults defaults)
        {
            _logger = logger;
            _defaults = defaults;
        }

        public override Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var artifacts = Enumerable.Empty<Artifact>()
                .Concat(context.GetArtifacts<ContainerArtifact>())
                .Concat(context.GetArtifacts<DocumentationSiteArtifact>())
                .Concat(context.GetArtifacts<ExecutableArtifact>())
                .Concat(context.GetArtifacts<HelmChartArtifact>())
                .Concat(context.GetArtifacts<NuGetArtifact>())
                .ToArray();

            if (!artifacts.Any())
            {
                _logger.LogWarning("No artifacts found for release, skipping release creation!");
                return Task.FromResult(EmptyRecipes);
            }

            var releaseTextBuilder = new StringBuilder();
            AddReleaseNotes(context, releaseTextBuilder);
            AddChangeLog(context, releaseTextBuilder);
            AddArtifactDescriptions(context, artifacts, releaseTextBuilder);
            AddGitHubChangeLink(context, releaseTextBuilder);

            var releaseFiles = BuildReleaseFiles(context, artifacts);
            var releaseText = releaseTextBuilder.ToString();

            return Task.FromResult<IReadOnlyCollection<Recipe>>(
            [
                new ReleaseRecipe(
                    releaseText,
                    releaseFiles.ToArray(),
                    new ReleaseArtifact(
                        releaseText,
                        releaseFiles.Select(f => f.Destination).ToArray()))
            ]);
        }

        private List<ReleaseFile> BuildReleaseFiles(
            IContext context,
            Artifact[] inputArtifacts)
        {
            var additionalSourceFiles = new[]
                {
                    Path.Combine(context.Ingredients.WorkingDirectory, "README.md"),
                    Path.Combine(context.Ingredients.WorkingDirectory, "LICENSE"),
                    Path.Combine(context.Ingredients.WorkingDirectory, "RELEASE_NOTES.md"),
                }
                .Where(File.Exists)
                .ToArray();

            var releaseFiles = new List<ReleaseFile>();

            foreach (var g in inputArtifacts.GroupBy(a => a.GetType()))
            {
                switch (g.Key)
                {
                    case { } t when t == typeof(DocumentationSiteArtifact):
                        {
                            foreach (var artifact in g)
                            {
                                var documentationSiteArtifact = (DocumentationSiteArtifact)artifact;
                                var fileName = $"documentation_v{context.Ingredients.Version}.zip";
                                releaseFiles.Add(new ReleaseFile(
                                    fileName,
                                    AppendFiles(documentationSiteArtifact.Path),
                                    Path.Combine(_defaults.BakeReleaseOutputDirectory, fileName)));
                            }
                        }
                        break;

                    case { } t when t == typeof(ExecutableArtifact):
                        {
                            foreach (var artifact in g)
                            {
                                var executableArtifact = (ExecutableArtifact) artifact;
                                var fileName = CalculateArtifactFileName(executableArtifact);
                                releaseFiles.Add(new ReleaseFile(
                                    fileName,
                                    AppendFiles(executableArtifact.Path),
                                    Path.Combine(_defaults.BakeReleaseOutputDirectory, fileName)));
                            }
                        }
                        break;
                }
            }

            return releaseFiles;

            string[] AppendFiles(params string[] paths)
            {
                return Enumerable.Empty<string>()
                    .Concat(additionalSourceFiles)
                    .Concat(paths)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        private static void AddReleaseNotes(IContext context, StringBuilder releaseText)
        {
            if (context.Ingredients.ReleaseNotes != null)
            {
                releaseText
                    .AppendLine("### Release notes")
                    .AppendLine(context.Ingredients.ReleaseNotes.Notes)
                    .AppendLine();
            }
        }

        private static void AddArtifactDescriptions(
            IContext _,
            Artifact[] artifacts,
            StringBuilder releaseText)
        {
            foreach (var g in artifacts.GroupBy(a => a.GetType()))
            {
                switch (g.Key)
                {
                    case { } t when t == typeof(ContainerArtifact):
                        {
                            releaseText.AppendLine("### Containers");
                            foreach (var artifact in g)
                            {
                                var containerArtifact = (ContainerArtifact) artifact;
                                releaseText.AppendLine($"* `{containerArtifact.Name}`");
                                foreach (var tag in containerArtifact.Tags)
                                {
                                    releaseText.AppendLine($"  * `{tag}`");
                                }
                            }
                        }
                        break;
                }
            }
        }

        private static void AddChangeLog(IContext context, StringBuilder releaseText)
        {
            if (context.Ingredients.Changelog == null || !context.Ingredients.Changelog.Changes.Any())
            {
                return;
            }

            foreach (var a in new[]
                 {
                     new {changeType = ChangeType.Other, title = "Changes"},
                     new {changeType = ChangeType.Dependency, title = "Updated dependencies"},
                 })
            {
                releaseText
                    .AppendLine($"#### {a.title}")
                    .AppendLine();

                foreach (var change in context.Ingredients.Changelog.Changes[a.changeType])
                {
                    releaseText.AppendLine($"* {change.Text}");
                }
                releaseText.AppendLine();
            }

            releaseText.AppendLine();
        }

        private static void AddGitHubChangeLink(IContext context, StringBuilder releaseText)
        {
            if (context.Ingredients is {GitHub: not null, Changelog: not null})
            {
                releaseText.AppendLine(
                    $"Full Changelog: {context.Ingredients.GitHub.Url.AbsoluteUri.TrimEnd('/')}/compare/{context.Ingredients.Changelog.PreviousReleaseTag.Sha}...{context.Ingredients.Git!.Sha}");
            }
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
