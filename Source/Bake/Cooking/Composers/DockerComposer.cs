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
using Bake.ValueObjects.BakeProjects;
using Bake.ValueObjects.Destinations;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Docker;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Composers
{
    public class DockerComposer : Composer
    {
        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.Container,
            };
        public override IReadOnlyCollection<ArtifactType> Consumes { get; } = new[]
            {
                ArtifactType.Dockerfile,
            };

        private readonly ILogger<DockerComposer> _logger;
        private readonly IDefaults _defaults;
        private readonly IFileSystem _fileSystem;
        private readonly IContainerTagParser _containerTagParser;
        private readonly IConventionInterpreter _conventionInterpreter;
        private readonly IBakeProjectParser _bakeProjectParser;

        public DockerComposer(
            ILogger<DockerComposer> logger,
            IDefaults defaults,
            IFileSystem fileSystem,
            IContainerTagParser containerTagParser,
            IConventionInterpreter conventionInterpreter,
            IBakeProjectParser bakeProjectParser)
        {
            _logger = logger;
            _defaults = defaults;
            _fileSystem = fileSystem;
            _containerTagParser = containerTagParser;
            _conventionInterpreter = conventionInterpreter;
            _bakeProjectParser = bakeProjectParser;
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
                .Concat(new[]{"bake.local"})
                .Distinct()
                .ToArray();
            var recipes = new List<Recipe>();

            foreach (var dockerFilePath in dockerFilePaths)
            {
                _logger.LogInformation(
                    "Located Dockerfile at these locations, scheduling build: {DockerFiles}",
                    dockerFilePaths);

                var directoryPath = Path.GetDirectoryName(dockerFilePath)!; // Is actual full path, terrible name for a method
                var bakeProject = await GetProjectAsync(
                    directoryPath,
                    cancellationToken);

                var containerName = string.IsNullOrEmpty(bakeProject?.Name)
                    ? Path.GetFileName(directoryPath)
                    : bakeProject.Name;

                recipes.AddRange(CreateRecipes(dockerFilePath, containerName, ingredients.Version, ingredients.PushContainerLatest, urls));
            }

            var dockerfileArtifacts = context
                .GetArtifacts<DockerFileArtifact>()
                .ToArray();
            if (dockerfileArtifacts.Any())
            {
                _logger.LogInformation(
                    "Other build steps will produce Dockerfile at these locations, scheduling build: {DockerFiles}",
                    dockerfileArtifacts.Select(a => a.Path).ToList());
                foreach (var dockerfileArtifact in dockerfileArtifacts)
                {
                    recipes.AddRange(CreateRecipes(dockerfileArtifact.Path, dockerfileArtifact.Name, ingredients.Version, ingredients.PushContainerLatest, urls));
                }
            }

            if (!recipes.Any())
            {
                return [];
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

        private async Task<BakeProject?> GetProjectAsync(
            string directory,
            CancellationToken cancellationToken)
        {
            var projectFilePath = Path.Combine(
                directory,
                "bake.yaml"); // TODO: Rework to align file name

            if (!File.Exists(projectFilePath))
            {
                return null;
            }

            var fileContent = await File.ReadAllTextAsync(projectFilePath, cancellationToken);
            return await _bakeProjectParser.ParseAsync(fileContent, cancellationToken);
        }
        
        private IEnumerable<Recipe> CreateRecipes(
            string dockerfilePath,
            string name,
            SemVer version,
            bool pushLatest,
            IEnumerable<string> urls)
        {
            var versions = new List<string>
                {
                    version.ToString(),
                };

            if (pushLatest)
            {
                versions.Add("latest");
            }

            var slug = name.ToSlug();

            var tags = (
                from u in urls
                from v in versions
                select $"{u}{slug}:{v}"
                ).ToArray();

            var secretMounts = new Dictionary<string, string>();
            var workingDirectory = Path.GetDirectoryName(dockerfilePath);

            _containerTagParser.Validate(tags);

            var npmRcPath = Path.Combine(workingDirectory!, ".npmrc");
            if (File.Exists(npmRcPath))
            {
                _logger.LogInformation(
                    "Found a .npmrc file at {FilePath}, parsing that as secret named 'npmrc' (note no '.' in front) to be used during build",
                    npmRcPath);
                secretMounts.Add("npmrc", ".npmrc");
            }

            yield return new DockerBuildRecipe(
                workingDirectory!,
                slug,
                tags,
                _defaults.DockerBuildCompress,
                secretMounts,
                new ContainerArtifact(
                    slug,
                    tags.ToArray()));
        }
    }
}
