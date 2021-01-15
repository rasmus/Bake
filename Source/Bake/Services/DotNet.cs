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
                    argument.FilePath,
                    "--nologo",
                    "--configuration", argument.Configuration,
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
                    argument.FilePath,
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
                     argument.FilePath,
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
                    argument.FilePath,
                    "--nologo",
                    "--configuration", argument.Configuration,
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

        public async Task<bool> PackAsync(
            DotNetPackArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "pack",
                    argument.FilePath,
                    "--nologo",
                    "--configuration", argument.Configuration,
                };

            AddIf(!argument.Restore, arguments, "--no-restore");
            AddIf(!argument.Build, arguments, "--no-build");
            AddIf(argument.IncludeSource, arguments, "--include-source");
            AddIf(argument.IncludeSymbols, arguments, "--include-symbols");
            AddIf(argument.Build, arguments, "--no-build");
            AddIf(!string.IsNullOrEmpty(argument.Version.Meta), arguments, "--version-suffix", argument.Version.Meta);

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);

            return result == 0;
        }

        private static void AddIf(bool predicate, List<string> arguments, params string[] args)
        {
            if (!predicate)
            {
                return;
            }

            arguments.AddRange(args);
        }
    }
}
