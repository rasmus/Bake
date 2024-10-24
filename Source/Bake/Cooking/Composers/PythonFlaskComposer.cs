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
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Python;

namespace Bake.Cooking.Composers
{
    public class PythonFlaskComposer : Composer
    {
        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.Dockerfile,
            };

        private readonly IFileSystem _fileSystem;
        private readonly IDockerLabels _dockerLabels;

        public PythonFlaskComposer(
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
            var files = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "app.py",
                cancellationToken);

            var labels = await _dockerLabels.FromIngredientsAsync(
                context.Ingredients,
                cancellationToken);

            return files
                .SelectMany(p => CreateRecipes(p, labels))
                .ToList();
        }

        private static IEnumerable<Recipe> CreateRecipes(
            string appFilePath,
            Dictionary<string, string> labels)
        {
            var directory = Path.GetDirectoryName(appFilePath)!;
            var dockerfilePath = Path.Combine(directory, "Dockerfile");
            var name = Path.GetFileName(directory).ToSlug();

            yield return new PythonFlaskDockerfileRecipe(
                directory,
                new DockerFileArtifact(name, dockerfilePath, labels));
        }
    }
}
