using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.ValueObjects.DotNet;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Composers
{
    public class DotNetComposer : IComposer
    {
        private readonly ICsProjParser _csProjParser;

        public DotNetComposer(
            ICsProjParser csProjParser)
        {
            _csProjParser = csProjParser;
        }

        public async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var solutionFilesTask = Task.Factory.StartNew(
                () => Directory.GetFiles(context.WorkingDirectory, "*.sln", SearchOption.AllDirectories),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            var projectFilesTask = Task.Factory.StartNew(
                () => Directory.GetFiles(context.WorkingDirectory, "*.csproj", SearchOption.AllDirectories),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            await Task.WhenAll(solutionFilesTask, projectFilesTask);

            var visualStudioSolutions = await Task.WhenAll(solutionFilesTask.Result
                .Select(p => LoadVisualStudioSolutionAsync(p, projectFilesTask.Result, cancellationToken)));

            return visualStudioSolutions
                .SelectMany(s => CreateRecipe(s, context.Version))
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

        private static IEnumerable<Recipe> CreateRecipe(
            VisualStudioSolution visualStudioSolution,
            SemVer version)
        {
            const string configuration = "Release";

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
                version);
            yield return new DotNetTestSolutionRecipe(
                visualStudioSolution.Path,
                false,
                false,
                configuration);

            foreach (var visualStudioProject in visualStudioSolution.Projects
                .Where(p => p.ShouldBePacked))
            {
                yield return new DotNetPackProjectRecipe(
                    visualStudioProject.Path,
                    false,
                    false,
                    true,
                    true,
                    configuration,
                    version);
            }
        }
    }
}
