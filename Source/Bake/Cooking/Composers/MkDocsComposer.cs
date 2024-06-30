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
using Bake.Services;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Destinations;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.MkDocs;
using Bake.ValueObjects.Recipes.Pip;
using File = System.IO.File;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Composers
{
    public class MkDocsComposer : Composer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDockerLabels _dockerLabels;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.DocumentationSite,
            };

        public MkDocsComposer(
            IFileSystem fileSystem,
            IDockerLabels dockerLabels)
        {
            _fileSystem = fileSystem;
            _dockerLabels = dockerLabels;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var mkDocsFilePaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "mkdocs.yml",
                cancellationToken);

            var labels = await _dockerLabels.FromIngredientsAsync(
                context.Ingredients,
                cancellationToken);
            var buildContainer = context.Ingredients.Destinations
                .OfType<ContainerDestination>()
                .Any(d => d.ArtifactType == ArtifactType.DocumentationSite);

            return mkDocsFilePaths
                .SelectMany(p => CreateRecipes(p, labels, buildContainer))
                .ToArray();
        }

        private static IEnumerable<Recipe> CreateRecipes(string mkDocsFilePath,
            Dictionary<string, string> labels,
            bool buildContainer)
        {
            var workingDirectory = Path.GetDirectoryName(mkDocsFilePath)!;
            var requirementsFilePath = Path.Combine(
                workingDirectory,
                "requirements.txt");

            if (File.Exists(requirementsFilePath))
            {
                yield return new PipInstallRequirementsRecipe(
                    requirementsFilePath,
                    workingDirectory);
            }

            var outputDirectory = $"bake-site-{DateTimeOffset.Now:yyyy-MM-dd-HHmmss-fff}";

            yield return new MkDocsBuildRecipe(
                workingDirectory,
                false,
                true,
                outputDirectory,
                new DocumentationSiteArtifact(
                    outputDirectory));

            var dockerFilePath = Path.Combine(
                workingDirectory,
                "Dockerfile");

            if (buildContainer)
            {
                yield return new MkDocsDockerFileRecipe(
                    outputDirectory,
                    new DockerFileArtifact(
                        "docs",
                        dockerFilePath,
                        labels));
            }
        }
    }
}
