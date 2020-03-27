using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace Bake.Services
{
    public class DotNet : IDotNet
    {
        private readonly ILogger<DotNet> _logger;
        private readonly ICli _cli;

        public DotNet(
            ILogger<DotNet> logger,
            ICli cli)
        {
            _logger = logger;
            _cli = cli;
        }

        public async Task CleanAsync(
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            await using var command = _cli.CreateCommand(
                "dotnet",
                workingDirectory,
                "clean",
                "--verbosity", "minimal",
                "--nologo");

            await command.ExecuteAsync(cancellationToken);
        }

        public async Task RestoreAsync(
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Clearing NuGet HTTP cache");
            await using var clearCommand = _cli.CreateCommand(
                "dotnet",
                workingDirectory,
                "nuget", "locals", "http-cache",
                "--clear");

            _logger.LogInformation("Restoring NuGet packages");
            await using var restoreCommand = _cli.CreateCommand(
                "dotnet",
                workingDirectory,
                "restore",
                "--verbosity", "minimal",
                "--locked-mode");

            await restoreCommand.ExecuteAsync(cancellationToken);
        }

        public async Task BuildAsync(
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            await using var command = _cli.CreateCommand(
                "dotnet",
                workingDirectory,
                "build",
                "--verbosity", "minimal",
                "--configuration", "Release",
                "--no-incremental",
                "--no-restore",
                "--nologo");

            await command.ExecuteAsync(cancellationToken);
        }
    }
}
