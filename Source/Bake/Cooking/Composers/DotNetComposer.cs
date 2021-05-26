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
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.DotNet;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.DotNet;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Composers
{
    public class DotNetComposer : IComposer
    {
        private readonly ICsProjParser _csProjParser;
        private readonly IConventionInterpreter _conventionInterpreter;

        public DotNetComposer(
            ICsProjParser csProjParser,
            IConventionInterpreter conventionInterpreter)
        {
            _csProjParser = csProjParser;
            _conventionInterpreter = conventionInterpreter;
        }

        public async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var solutionFilesTask = Task.Factory.StartNew(
                () => Directory.GetFiles(context.Ingredients.WorkingDirectory, "*.sln", SearchOption.AllDirectories),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            var projectFilesTask = Task.Factory.StartNew(
                () => Directory.GetFiles(context.Ingredients.WorkingDirectory, "*.csproj", SearchOption.AllDirectories),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            await Task.WhenAll(solutionFilesTask, projectFilesTask);

            var visualStudioSolutions = await Task.WhenAll(solutionFilesTask.Result
                .Select(p => LoadVisualStudioSolutionAsync(p, projectFilesTask.Result, cancellationToken)));

            return visualStudioSolutions
                .SelectMany(s => CreateRecipe(context, s))
                .ToList();
        }

        private async Task<VisualStudioSolution> LoadVisualStudioSolutionAsync(
            string solutionPath,
            IEnumerable<string> projectFiles,
            CancellationToken cancellationToken)
        {
            var solutionFileContent = await File.ReadAllTextAsync(solutionPath, cancellationToken);

            var tasks = projectFiles
                .Where(f => solutionFileContent.Contains(Path.GetFileName(f)))
                .Select(f => LoadVisualStudioProjectAsync(f, cancellationToken))
                .ToList();

            var projectFilesInSolution = await Task.WhenAll(tasks);

            return new VisualStudioSolution(
                solutionPath,
                projectFilesInSolution);
        }

        private async Task<VisualStudioProject> LoadVisualStudioProjectAsync(
            string projectPath,
            CancellationToken cancellationToken)
        {
            var csProj = await _csProjParser.ParseAsync(
                projectPath,
                cancellationToken);

            return new VisualStudioProject(
                projectPath,
                csProj);
        }

        private IEnumerable<Recipe> CreateRecipe(
            IContext context,
            VisualStudioSolution visualStudioSolution)
        {
            const string configuration = "Release";

            var ingredients = context.Ingredients;
            var description = ingredients.Git != null
                ? $"SHA:{ingredients.Git.Sha}"
                : visualStudioSolution.Name;

            yield return new DotNetCleanSolutionRecipe(
                visualStudioSolution.Path,
                configuration);
            yield return new DotNetRestoreSolutionRecipe(
                visualStudioSolution.Path,
                true);
            yield return new DotNetBuildSolutionRecipe(
                visualStudioSolution.Path,
                configuration,
                false,
                false,
                description,
                ingredients.Version);
            yield return new DotNetTestSolutionRecipe(
                visualStudioSolution.Path,
                false,
                false,
                configuration);

            foreach (var visualStudioProject in visualStudioSolution.Projects
                .Where(p => p.ShouldBePacked))
            {
                var artifacts = new Artifact[]
                    {
                        new FileArtifact(
                            new ArtifactKey("nuget", visualStudioProject.Name),
                            CalculateNuGetPath(ingredients, visualStudioProject, configuration))
                    };

                yield return new DotNetPackProjectRecipe(
                    visualStudioProject.Path,
                    false,
                    false,
                    true,
                    true,
                    configuration,
                    ingredients.Version,
                    artifacts);
            }

            if (_conventionInterpreter.ShouldArtifactsBePublished(ingredients.Convention))
            {
                foreach (var visualStudioProject in visualStudioSolution.Projects
                    .Where(p => p.ShouldBePushed))
                {
                    yield return new DotNetNuGetPushRecipe(
                        CalculateNuGetPath(ingredients, visualStudioProject, configuration),
                        "acd0b30512ac4fa39f62eb7a61fcf56c",
                        "http://localhost:5555/v3/index.json"
                        //"https://api.nuget.org/v3/index.json"
                    );
                }
            }

            foreach (var visualStudioProject in visualStudioSolution.Projects.Where(p => p.CsProj.IsPublishable))
            {
                var path = Path.Combine("bin", configuration, "publish", "AnyCpu");
                yield return new DotNetPublishRecipe(
                    visualStudioProject.Path,
                    false,
                    false,
                    false,
                    configuration,
                    DotNetTargetRuntime.NotConfigured,
                    path,
                    new Artifact[]
                    {
                        new DirectoryArtifact(
                            new ArtifactKey("dotnet-publish", visualStudioProject.Name),
                            Path.Combine(visualStudioProject.Directory, path))
                    });

                yield return new DotNetDockerFileRecipe(
                    visualStudioProject.Path,
                    new Artifact[]
                    {
                        new FileArtifact(
                            new ArtifactKey("dockerfile", visualStudioProject.Name),
                            Path.Combine(visualStudioProject.Directory, "Dockerfile"))
                    });
            }

            foreach (var visualStudioProject in visualStudioSolution.Projects
                .Where(p => p.CsProj.PackAsTool))
            foreach (var runtime in new []{DotNetTargetRuntime.Linux64, DotNetTargetRuntime.Windows64})
            {
                var path = Path.Combine("bin", configuration, "publish", runtime.ToName());
                yield return new DotNetPublishRecipe(
                    visualStudioProject.Path,
                    true,
                    false,
                    true,
                    configuration,
                    runtime,
                    path,
                    new Artifact[]
                    {
                        new DirectoryArtifact(
                            new ArtifactKey($"dotnet-tool-{runtime.ToName()}", visualStudioProject.Name),
                            Path.Combine(visualStudioProject.Directory, path))
                    });
            }
        }

        private static string CalculateNuGetPath(
            ValueObjects.Ingredients ingredients,
            VisualStudioProject visualStudioProject,
            string configuration)
        {
            return Path.Combine(
                visualStudioProject.Directory,
                "bin",
                configuration,
                $"{visualStudioProject.Name}.{ingredients.Version}.nupkg");
        }
    }
}
