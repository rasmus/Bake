using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;

namespace Bake.Cooking.Composers
{
    public class NodeJsComposer : Composer
    {
        private readonly IFileSystem _fileSystem;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.Dockerfile,
            };

        public NodeJsComposer(
            IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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
                .Select(p => CreateRecipesAsync(p, cancellationToken))
                .ToArray();

            var recipes = await Task.WhenAll(recipeTasks);

            return recipes
                .SelectMany(r => r)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<Recipe>> CreateRecipesAsync(
            string packageJsonPath,
            CancellationToken cancellationToken)
        {
            var recipes = new List<Recipe>();



            return recipes;
        }
    }
}
