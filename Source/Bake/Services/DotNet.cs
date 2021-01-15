using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Extensions;
using Bake.Services.DotNetArgumentBuilders;

// ReSharper disable StringLiteralTypo

namespace Bake.Services
{
    public class DotNet : IDotNet
    {
        private readonly IRunnerFactory _runnerFactory;

        public DotNet(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public async Task<bool> ClearNuGetLocalsAsync(
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "nuget",
                    "locals",
                    "http-cache",
                    "--clear"
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                Directory.GetCurrentDirectory(),
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);

            return result == 0;
        }

        public async Task<bool> CleanAsync(
            DotNetCleanArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "clean",
                    argument.SolutionPath,
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);

            return result == 0;
        }

        public async Task<bool> RestoreAsync(
            DotNetRestoreArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "restore",
                    argument.SolutionPath,
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);

            return result == 0;
        }

        public async Task<bool> BuildAsync(
            DotNetBuildArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                     "build",
                     argument.SolutionPath,
                     "--nologo",
                     "--configuration", argument.Configuration,
                    $"-p:Version={argument.Version.LegacyVersion}",
                    $"-p:AssemblyVersion={argument.Version.LegacyVersion}",
                    $"-p:AssemblyFileVersion={argument.Version.LegacyVersion}",
                };

            AddIf(!argument.Incremental, arguments, "--no-incremental");
            AddIf(!argument.Restore, arguments, "--no-restore");

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);

            return result == 0;
        }

        public async Task<bool> TestAsync(
            DotNetTestArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "test",
                    argument.SolutionPath,
                    "--nologo"
                };

            AddIf(!argument.Restore, arguments, "--no-restore");
            AddIf(!argument.Build, arguments, "--no-build");

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);

            return result == 0;
        }


        private static void AddIf(bool predicate, List<string> arguments, string arg)
        {
            if (!predicate)
            {
                return;
            }

            arguments.Add(arg);
        }
    }
}
