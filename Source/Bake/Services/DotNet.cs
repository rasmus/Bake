using System.Threading;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace Bake.Services
{
    public class DotNet : IDotNet
    {
        private readonly ICli _cli;

        public DotNet(
            ICli cli)
        {
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
            await using var command = _cli.CreateCommand(
                "dotnet",
                workingDirectory,
                "restore",
                "--verbosity", "minimal",
                "--locked-mode");

            await command.ExecuteAsync(cancellationToken);
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
