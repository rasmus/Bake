using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Composers
{
    public class DotNetComposer : IComposer
    {
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

        private static async Task<VisualStudioSolution> LoadVisualStudioSolutionAsync(
            string solutionPath,
            IEnumerable<string> projectFiles,
            CancellationToken cancellationToken)
        {
            var solutionFileContent = await File.ReadAllTextAsync(solutionPath, cancellationToken);

            // TODO: Handle duplicate names in different locations
            var projectFilesInSolution = projectFiles
                .Where(f => solutionFileContent.Contains(Path.GetFileName(f)))
                .Select(f => new VisualStudioProject(f));

            return new VisualStudioSolution(
                solutionPath,
                projectFilesInSolution);
        }

        private static IEnumerable<Recipe> CreateRecipe(
            VisualStudioSolution visualStudioSolution,
            SemVer version)
        {
            yield return new DotNetCleanSolution(
                visualStudioSolution.Path);
            yield return new DotNetRestoreSolution(
                visualStudioSolution.Path,
                true);
            yield return new DotNetBuildSolution(
                visualStudioSolution.Path,
                "Release",
                false,
                false,
                version);
            yield return new DotNetTestSolution(
                visualStudioSolution.Path,
                false,
                false);
        }
    }
}
