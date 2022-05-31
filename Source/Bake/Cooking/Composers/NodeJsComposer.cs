// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.NodeJS;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Composers
{
    public class NodeJsComposer : Composer
    {
        private readonly ILogger<NodeJsComposer> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IDockerLabels _dockerLabels;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.Dockerfile,
            };

        public NodeJsComposer(
            ILogger<NodeJsComposer> logger,
            IFileSystem fileSystem,
            IDockerLabels dockerLabels)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _dockerLabels = dockerLabels;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var packageJsonPaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "package.json",
                cancellationToken);

            if (!packageJsonPaths.Any())
            {
                return Array.Empty<Recipe>();
            }

            var recipeTasks = packageJsonPaths
                .Select(p => CreateRecipesAsync(context.Ingredients, p, cancellationToken))
                .ToArray();

            var recipes = await Task.WhenAll(recipeTasks);

            return recipes
                .SelectMany(r => r)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<Recipe>> CreateRecipesAsync(
            ValueObjects.Ingredients ingredients,
            string packageJsonPath,
            CancellationToken cancellationToken)
        {
            var workingDirectory = Path.GetDirectoryName(packageJsonPath);
            var parts = packageJsonPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Any(p => string.Equals("node_modules", p, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogDebug(
                    "Skipping directory {DirectoryPath} as its within a 'node_modules' folder",
                    workingDirectory);
                return Array.Empty<Recipe>();
            }

            // TODO: Actually figure out if is a service or NPM package

            var labels = await _dockerLabels.FromIngredientsAsync(
                ingredients,
                cancellationToken);

            var recipes = new List<Recipe>
            {
                new NpmCIRecipe(workingDirectory),
                new NodeJSDockerfileRecipe(
                    workingDirectory,
                    labels,
                    new DockerFileArtifact(
                        Path.GetFileName(workingDirectory).ToSlug(), // TODO: Need to pick up name from package.json
                        Path.Combine(workingDirectory, "Dockerfile")))
            };

            return recipes;
        }
    }
}
