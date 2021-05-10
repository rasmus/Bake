using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Commands.Plan
{
    [CommandVerb("plan")]
    public class PlanCommand : ICommand
    {
        private readonly ILogger<PlanCommand> _logger;
        private readonly IEditor _editor;
        private readonly IYaml _yaml;

        public PlanCommand(
            ILogger<PlanCommand> logger,
            IEditor editor,
            IYaml yaml)
        {
            _logger = logger;
            _editor = editor;
            _yaml = yaml;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> ExecuteAsync(
            SemVer buildVersion,
            string planPath,
            CancellationToken cancellationToken,
            bool force = false)
        {
            planPath = Path.GetFullPath(planPath);
            _logger.LogInformation(
                "Saving Bake plan to {PlanPath} with version {Version}",
                planPath,
                buildVersion);

            var directoryPath = Path.GetDirectoryName(planPath);
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogInformation(
                    "No directory located at {DirectoryPath}, creating it",
                    directoryPath);
                Directory.CreateDirectory(directoryPath);
            }

            if (File.Exists(planPath))
            {
                if (force)
                {
                    _logger.LogWarning(
                        "File already exists at {PlanPath}, but asked to force the operation so deleting it",
                        planPath);
                    File.Delete(planPath);
                }
                else
                {
                    _logger.LogCritical(
                        "File already exists at {PlanPath}");
                    return ExitCodes.Plan.PlanFileAlreadyExists;
                }
            }

            var content = new Context(
                Ingredients.New(
                    buildVersion,
                    Directory.GetCurrentDirectory()));

            var book = await _editor.ComposeAsync(
                content,
                cancellationToken);

            var yaml = await _yaml.SerializeAsync(
                book,
                cancellationToken);

            _logger.LogInformation(
                "Writing plan to {PlanPath}",
                planPath);
            await File.WriteAllTextAsync(
                planPath,
                yaml,
                Encoding.UTF8,
                cancellationToken);

            return 0;
        }
    }
}
