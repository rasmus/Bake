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

using System.Text;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Release;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Composers
{
    public class ReleaseComposer : Composer
    {
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

        public ReleaseComposer(
            ILogger<ReleaseComposer> logger)
        {
            _logger = logger;
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

            var releaseText = new StringBuilder();
            AddReleaseNotes(context, releaseText);
            AddChangeLog(context, releaseText);
            AddArtifactDescriptions(context, artifacts, releaseText);
            AddGitHubChangeLink(context, releaseText);

            return Task.FromResult<IReadOnlyCollection<Recipe>>(
            [
                new ReleaseRecipe(new ReleaseArtifact(releaseText.ToString()))
            ]);
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
    }
}
