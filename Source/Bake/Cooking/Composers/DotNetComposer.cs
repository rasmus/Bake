// MIT License
// 
// Copyright (c) 2021-2026 Rasmus Mikkelsen
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
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Destinations;
using Bake.ValueObjects.DotNet;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.DotNet;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml.Linq;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Composers
{
    public class DotNetComposer : Composer
    {
        public const int MaximumNuGetDescription = 4000;
        private static readonly IReadOnlyDictionary<string, string> DefaultProperties = new Dictionary<string, string>
            {
                // We really don't want builds to fail due to old versions
                ["CheckEolTargetFramework"] = "false",

                // Make the NuGet package easier for everyone to debug
                ["EmbedUntrackedSources"] = "true",
                ["PublishRepositoryUrl"] = "true",
                ["IncludeSymbols"] = "true",
                ["DebugType"] = "portable",
                ["SymbolPackageFormat"] = "snupkg",
        };

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.Executable,
                ArtifactType.Dockerfile,
                ArtifactType.NuGet,
                ArtifactType.DotNetPublishedDirectory,
            };

        private readonly ILogger<DotNetComposer> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ICsProjParser _csProjParser;
        private readonly IConventionInterpreter _conventionInterpreter;
        private readonly IDefaults _defaults;
        private readonly IDockerLabels _dockerLabels;
        private readonly IDescriptionLimiter _descriptionLimiter;
        private readonly IBakeProjectParser _bakeProjectParser;

        public DotNetComposer(
            ILogger<DotNetComposer> logger,
            IFileSystem fileSystem,
            ICsProjParser csProjParser,
            IConventionInterpreter conventionInterpreter,
            IDefaults defaults,
            IDockerLabels dockerLabels,
            IDescriptionLimiter descriptionLimiter,
            IBakeProjectParser bakeProjectParser)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _csProjParser = csProjParser;
            _conventionInterpreter = conventionInterpreter;
            _defaults = defaults;
            _dockerLabels = dockerLabels;
            _descriptionLimiter = descriptionLimiter;
            _bakeProjectParser = bakeProjectParser;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var slnxFiles = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "*.slnx",
                cancellationToken);

            IReadOnlyCollection<VisualStudioSolution> visualStudioSolutions;

            if (slnxFiles.Any())
            {
                visualStudioSolutions = await Task.WhenAll(slnxFiles
                    .Select(p => LoadVisualStudioSolutionFromSlnxAsync(p, cancellationToken)));
            }
            else
            {
                var solutionFilesTask = _fileSystem.FindFilesAsync(
                    context.Ingredients.WorkingDirectory,
                    "*.sln",
                    cancellationToken);
                var projectFilesTask = _fileSystem.FindFilesAsync(
                    context.Ingredients.WorkingDirectory,
                    "*.csproj",
                    cancellationToken);
                await Task.WhenAll(solutionFilesTask, projectFilesTask);

                visualStudioSolutions = await Task.WhenAll(solutionFilesTask.Result
                    .Select(p => LoadVisualStudioSolutionAsync(p, projectFilesTask.Result, cancellationToken)));
            }

            var labels = await _dockerLabels.FromIngredientsAsync(
                context.Ingredients,
                cancellationToken);

            return visualStudioSolutions
                .SelectMany(s => CreateRecipe(context, labels, s))
                .ToList();
        }

        private async Task<VisualStudioSolution> LoadVisualStudioSolutionAsync(
            string solutionPath,
            IEnumerable<string> projectFiles,
            CancellationToken cancellationToken)
        {
            var solutionFileContent = await System.IO.File.ReadAllTextAsync(solutionPath, cancellationToken);

            var tasks = projectFiles
                .Where(f => solutionFileContent.Contains(Path.GetFileName(f)))
                .Select(f => LoadVisualStudioProjectAsync(f, cancellationToken))
                .ToList();

            var projectFilesInSolution = await Task.WhenAll(tasks);

            return new VisualStudioSolution(
                solutionPath,
                projectFilesInSolution);
        }

        private async Task<VisualStudioSolution> LoadVisualStudioSolutionFromSlnxAsync(
            string slnxPath,
            CancellationToken cancellationToken)
        {
            var content = await System.IO.File.ReadAllTextAsync(slnxPath, cancellationToken);
            var document = XDocument.Parse(content);
            var solutionDirectory = Path.GetDirectoryName(slnxPath)!;

            var projectPaths = document.Descendants("Project")
                .Select(e => e.Attribute("Path")?.Value)
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => Path.GetFullPath(Path.Combine(solutionDirectory, p!)))
                .Where(p => p.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var projects = await Task.WhenAll(projectPaths
                .Select(p => LoadVisualStudioProjectAsync(p, cancellationToken)));

            return new VisualStudioSolution(slnxPath, projects);
        }

        private async Task<VisualStudioProject> LoadVisualStudioProjectAsync(
            string projectPath,
            CancellationToken cancellationToken)
        {
            var csProj = await _csProjParser.ParseAsync(
                projectPath,
                cancellationToken);

            var bakeProjectFilePath = Path.Combine(
                Path.GetDirectoryName(projectPath)!,
                "bake.yaml");
            string? preferredName = null;

            if (_fileSystem.FileExists(bakeProjectFilePath))
            {
                var yaml = await _fileSystem.ReadAllTextAsync(bakeProjectFilePath, cancellationToken);
                var bakeProject = await _bakeProjectParser.ParseAsync(yaml, cancellationToken);
                if (!string.IsNullOrEmpty(bakeProject.Name))
                {
                    preferredName = bakeProject.Name;
                }
            }

            var name = Path.GetFileNameWithoutExtension(projectPath);
            if (string.IsNullOrEmpty(preferredName))
            {
                preferredName = name;
            }

            return new VisualStudioProject(
                projectPath,
                name,
                preferredName,
                csProj);
        }

        private IEnumerable<Recipe> CreateRecipe(
            IContext context,
            Dictionary<string, string> labels,
            VisualStudioSolution visualStudioSolution)
        {
            const string configuration = "Release";

            var ingredients = context.Ingredients;
            var properties = CreateProperties(ingredients, visualStudioSolution);

            yield return new DotNetCleanSolutionRecipe(
                visualStudioSolution.Path,
                configuration);

            yield return CreateRestoreRecipe(visualStudioSolution, ingredients);

            yield return CreateBuildRecipe(
                visualStudioSolution,
                ingredients,
                configuration,
                properties);

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
                    visualStudioProject.CsProj.IncludeSymbols.GetValueOrDefault(true),
                    true,
                    configuration,
                    ingredients.Version,
                    properties,
                    new NuGetArtifact(
                        CalculateNuGetPath(ingredients, visualStudioProject, configuration)));
            }

            if (_conventionInterpreter.ShouldArtifactsBePublished(ingredients.Convention))
            {
                var nuGetRegistryDestinations = ingredients.Destinations
                    .OfType<NuGetRegistryDestination>()
                    .ToList();

                if (!nuGetRegistryDestinations.Any())
                {
                    _logger.LogWarning(
                        "Convention {Convention} dictates that artifacts should be published, but {DestinationName} is specificed",
                        ingredients.Convention,
                        Names.Destinations.NuGetRegistry);
                }
                else
                {
                    foreach (var nuGetRegistryDestination in nuGetRegistryDestinations)
                    foreach (var visualStudioProject in visualStudioSolution.Projects.Where(p => p.ShouldBePushed))
                    {
                        yield return new DotNetNuGetPushRecipe(
                            CalculateNuGetPath(ingredients, visualStudioProject, configuration),
                            _defaults.NuGetSkipDuplicate,
                            nuGetRegistryDestination.Url);
                    }
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
                    Platform.Any,
                    path,
                    ingredients.Version,
                    properties,
                    new DirectoryArtifact(
                        Path.Combine(visualStudioProject.Directory, path)));

                var moniker = visualStudioProject.CsProj.TargetFrameworkVersions
                    .Where(v => v.Framework == TargetFramework.Net)
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault()?.Moniker;
                if (string.IsNullOrEmpty(moniker))
                {
                    throw new Exception($"Could not find a moniker for project: {visualStudioProject.Path}");
                }

                yield return new DotNetDockerFileRecipe(
                    visualStudioProject.Path,
                    path.Replace("\\", "/"),
                    $"{visualStudioProject.AssemblyName}.dll",
                    moniker,
                    new DockerFileArtifact(
                        visualStudioProject.PreferredName,
                        Path.Combine(visualStudioProject.Directory, "Dockerfile"),
                        labels));
            }

            foreach (var visualStudioProject in visualStudioSolution.Projects.Where(p => p.CsProj.PackAsTool))
            foreach (var targetPlatform in ingredients.Platforms)
            {
                var useFrameworkDependentPublish = OperatingSystem.IsLinux() &&
                                                   targetPlatform.Os == ExecutableOperatingSystem.MacOSX &&
                                                   targetPlatform.Arch == ExecutableArchitecture.Arm64;

                var path = Path.Combine(
                    "bin",
                    configuration,
                    "publish",
                    targetPlatform.GetDotNetRuntimeIdentifier());

                var outputDirectory = Path.Combine(visualStudioProject.Directory, path);
                var executableFileName = targetPlatform.Os == ExecutableOperatingSystem.Windows
                    ? $"{visualStudioProject.AssemblyName}.exe"
                    : useFrameworkDependentPublish
                        ? $"{visualStudioProject.AssemblyName}.dll"
                        : visualStudioProject.AssemblyName;
                var publishProperties = useFrameworkDependentPublish
                    ? new Dictionary<string, string>(properties)
                        {
                            ["UseAppHost"] = "false"
                        }
                    : properties;

                yield return new DotNetPublishRecipe(
                    visualStudioProject.Path,
                    !useFrameworkDependentPublish,
                    !useFrameworkDependentPublish,
                    true,
                    configuration,
                    targetPlatform,
                    path,
                    ingredients.Version,
                    publishProperties,
                    new ExecutableArtifact(
                        visualStudioProject.CsProj.ToolCommandName,
                        outputDirectory,
                        executableFileName,
                        targetPlatform));
            }
        }

        private Recipe CreateRestoreRecipe(
            VisualStudioSolution visualStudioSolution,
            ValueObjects.Ingredients ingredients)
        {
            var sources = new List<string>();

            var gitHub = ingredients.GitHub;
            if (gitHub != null)
            {
                sources.Add(_defaults.GitHubNuGetRegistry.Replace("/OWNER/", $"/{gitHub.Owner}/"));
            }

            return new DotNetRestoreSolutionRecipe(
                visualStudioSolution.Path,
                true,
                sources.ToArray());
        }

        private static Recipe CreateBuildRecipe(
            VisualStudioSolution visualStudioSolution,
            ValueObjects.Ingredients ingredients,
            string configuration,
            Dictionary<string, string> properties)
        {
            return new DotNetBuildSolutionRecipe(
                visualStudioSolution.Path,
                configuration,
                false,
                false,
                ingredients.Version,
                properties);
        }

        private Dictionary<string, string> CreateProperties(
            ValueObjects.Ingredients ingredients,
            VisualStudioSolution visualStudioSolution)
        {
            var properties = DefaultProperties.ToDictionary(kv => kv.Key, kv => kv.Value);

            if (ingredients.ReleaseNotes != null)
            {
                properties["PackageReleaseNotes"] = ingredients.ReleaseNotes.Notes;
            }

            if (ingredients.Git != null)
            {
                var git = ingredients.Git;
                properties["RepositoryType"] = "git";
                properties["RepositoryCommit"] = git.Sha;
                properties["RepositoryUrl"] = git.OriginUrl.AbsoluteUri;
                properties["PublishRepositoryUrl"] = "true";
            }
            if (ingredients.GitHub != null)
            {
                var gitHub = ingredients.GitHub;
                properties["Authors"] = gitHub.Owner;

                // Remove the git value and use GitHub
                properties["RepositoryUrl"] = gitHub.Url.AbsoluteUri;
            }

            var legacyVersion = ingredients.Version.LegacyVersion.ToString();

            // CRITICAL for NuGet packages!
            // Setting the AssemblyVersion to [MAJOR].0.0.0 ensures that the assembly version remains stable
            // across minor and patch updates, which is important for binding redirects and compatibility.
            properties["AssemblyVersion"] = $"{ingredients.Version.Major}.0.0.0";

            properties["Version"] = legacyVersion;
            properties["ApplicationVersion"] = legacyVersion;
            properties["FileVersion"] = legacyVersion;
            properties["AssemblyFileVersion"] = legacyVersion;
            properties["Description"] = BuildDescription(visualStudioSolution, ingredients);

            return properties;
        }

        private string BuildDescription(
            VisualStudioSolution visualStudioSolution,
            ValueObjects.Ingredients ingredients)
        {
            if (ingredients.Description != null)
            {
                return _descriptionLimiter.Limit(ingredients.Description.Text, MaximumNuGetDescription);
            }

            var elements = new Dictionary<string, string>
                {
                    ["SolutionName"] = visualStudioSolution.Name,
                    ["BuildTime"] = DateTimeOffset.Now.ToString("O"),
                    ["Version"] = ingredients.Version.ToString(),
                    ["LegacyVersion"] = ingredients.Version.LegacyVersion.ToString(),
                    ["BakeVersion"] = Constants.Version,
                };

            if (ingredients.ReleaseNotes != null)
            {
                elements["ReleaseNotes"] = ingredients.ReleaseNotes.Notes;
            }

            if (ingredients.Git != null)
            {
                elements["GitSha"] = ingredients.Git.Sha;
            }

            if (ingredients.GitHub != null)
            {
                elements["GitHubRepositoryUrl"] = ingredients.GitHub.Url.AbsoluteUri;
                elements["GitHubOwner"] = ingredients.GitHub.Owner;
                elements["GitHubRepository"] = ingredients.GitHub.Repository;
            }

            return JsonConvert.SerializeObject(
                elements,
                Formatting.Indented);
        }

        private static string CalculateNuGetPath(
            ValueObjects.Ingredients ingredients,
            VisualStudioProject visualStudioProject,
            string configuration)
        {
            var packageId = string.IsNullOrEmpty(visualStudioProject.CsProj.PackageId)
                ? visualStudioProject.Name
                : visualStudioProject.CsProj.PackageId;

            return Path.Combine(
                visualStudioProject.Directory,
                "bin",
                configuration,
                $"{packageId}.{ingredients.Version}.nupkg");
        }
    }
}
