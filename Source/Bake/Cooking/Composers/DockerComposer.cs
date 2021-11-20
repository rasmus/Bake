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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Destinations;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Docker;

namespace Bake.Cooking.Composers
{
    public class DockerComposer : Composer
    {
        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.DockerContainer,
            };
        public override IReadOnlyCollection<ArtifactType> Consumes { get; } = new[]
            {
                ArtifactType.Dockerfile,
            };

        private readonly IFileSystem _fileSystem;
        private readonly IContainerTagParser _containerTagParser;
        private readonly IConventionInterpreter _conventionInterpreter;

        public DockerComposer(
            IFileSystem fileSystem,
            IContainerTagParser containerTagParser,
            IConventionInterpreter conventionInterpreter)
        {
            _fileSystem = fileSystem;
            _containerTagParser = containerTagParser;
            _conventionInterpreter = conventionInterpreter;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var ingredients = context.Ingredients;
            var dockerFilePaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "Dockerfile",
                cancellationToken);

            var urls = ingredients.Destinations
                .OfType<ContainerRegistryDestination>()
                .Select(d => d.Url)
                .Distinct()
                .ToArray();
            var recipes = new List<Recipe>();

            foreach (var dockerFilePath in dockerFilePaths)
            {
                recipes.AddRange(CreateRecipes(dockerFilePath, ingredients.Version, urls));
            }

            foreach (var fileArtifact in context
                .GetArtifacts<FileArtifact>()
                .Where(a => a.Key.Type == ArtifactType.Dockerfile))
            {
                recipes.AddRange(CreateRecipes(fileArtifact.Path, ingredients.Version, urls));
            }

            if (_conventionInterpreter.ShouldArtifactsBePublished(ingredients.Convention))
            {
                var tags = recipes
                    .OfType<DockerBuildRecipe>()
                    .SelectMany(b => b.Tags)
                    .Distinct();
                recipes.Add(new DockerPushRecipe(tags));
            }

            return recipes;
        }

        private IEnumerable<Recipe> CreateRecipes(
            string path,
            SemVer version,
            IEnumerable<string> urls)
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(path));
            var slug = directoryName.ToSlug();
            var tags = urls
                .Select(u => $"{u}{slug}:{version}")
                .ToArray();

            _containerTagParser.Validate(tags);

            yield return new DockerBuildRecipe(
                path,
                slug,
                tags);
        }
    }
}
