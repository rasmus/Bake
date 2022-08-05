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
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.BakeProjects;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Go;

namespace Bake.Cooking.Composers
{
    public class GoComposer : Composer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGoModParser _goModParser;
        private readonly IBakeProjectParser _bakeProjectParser;
        private readonly IDockerLabels _dockerLabels;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.Executable,
                ArtifactType.Dockerfile,
            };

        public GoComposer(
            IFileSystem fileSystem,
            IGoModParser goModParser,
            IBakeProjectParser bakeProjectParser,
            IDockerLabels dockerLabels)
        {
            _fileSystem = fileSystem;
            _goModParser = goModParser;
            _bakeProjectParser = bakeProjectParser;
            _dockerLabels = dockerLabels;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var goModFilePaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "go.mod",
                cancellationToken);
            var labels = (await _dockerLabels.FromIngredientsAsync(
                context.Ingredients,
                cancellationToken))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return (await Task.WhenAll(
                    goModFilePaths
                        .Select(Path.GetDirectoryName)
                        .Select(p => CreateRecipesAsync(p, context, labels, cancellationToken))))
                .SelectMany(l => l)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<Recipe>> CreateRecipesAsync(
            string directoryPath,
            IContext context,
            Dictionary<string, string> labels,
            CancellationToken cancellationToken)
        {
            var goModPath = Path.Combine(directoryPath, "go.mod");
            var goModContent = await _fileSystem.ReadAllTextAsync(
                goModPath,
                cancellationToken);
            var projectType = BakeProjectType.Tool;
            var servicePort = -1;

            var bakeProjectPath = Path.Combine(directoryPath, "bake.yaml"); // TODO: Rework to align file name
            if (System.IO.File.Exists(bakeProjectPath))
            {
                var bakeProjectContent = await _fileSystem.ReadAllTextAsync(
                    bakeProjectPath,
                    cancellationToken);
                var bakeProject = await _bakeProjectParser.ParseAsync(
                    bakeProjectContent,
                    cancellationToken);
                if (bakeProject.Type == BakeProjectType.Service)
                {
                    projectType = BakeProjectType.Service;
                    servicePort = bakeProject.Service.Port;
                }
            }

            if (!_goModParser.TryParse(goModContent, out var goModuleName))
            {
                throw new InvalidOperationException($"Invalid content of '{goModPath}':{Environment.NewLine}{goModContent}");
            }

            var windowsOutput = $"{goModuleName.Name}.exe";

            var recipes = new List<Recipe>
                {
                    new GoTestRecipe(
                        directoryPath),
                };

            recipes.AddRange(context.Ingredients.Platforms
                .Select(p =>
                {
                    var output = p.Os == ExecutableOperatingSystem.Windows
                        ? Path.Combine(p.GetSlug(), windowsOutput)
                        : Path.Combine(p.GetSlug(), goModuleName.Name);

                    return new GoBuildRecipe(
                        output,
                        directoryPath,
                        p,
                        new ExecutableArtifact(
                            output,
                            Path.Combine(directoryPath, output),
                            p));
                }));

            var linuxBuildRecipe = recipes
                .OfType<GoBuildRecipe>()
                .SingleOrDefault(a => a.Platform.Os == ExecutableOperatingSystem.Linux);

            if (projectType == BakeProjectType.Service && linuxBuildRecipe != null)
            {
                recipes.Add(new GoDockerFileRecipe(
                    goModuleName.Name,
                    servicePort,
                    directoryPath,
                    labels,
                    linuxBuildRecipe.Output,
                    new DockerFileArtifact(
                        goModuleName.Name,
                        Path.Combine(directoryPath, "Dockerfile"))));
            }

            return recipes;
        }
    }
}
