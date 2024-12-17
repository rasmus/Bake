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
            ArtifactType.NuGet,
            ArtifactType.Executable,
            ArtifactType.DocumentationSite,
            ArtifactType.Container
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
                .Concat(context.GetArtifacts<ExecutableArtifact>())
                .Concat(context.GetArtifacts<DocumentationSiteArtifact>())
                .Concat(context.GetArtifacts<ContainerArtifact>())
                .Concat(context.GetArtifacts<NuGetArtifact>())
                .ToArray();

            if (!artifacts.Any())
            {
                _logger.LogWarning("No artifacts found for release, skipping release creation!");
                return Task.FromResult(EmptyRecipes);
            }

            return Task.FromResult<IReadOnlyCollection<Recipe>>(
            [
                new ReleaseRecipe(
                    context.Ingredients.Version,
                    context.Ingredients.ReleaseNotes!,
                    artifacts)
            ]);
        }
    }
}
