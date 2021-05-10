using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Commands.Apply
{
    [CommandVerb("apply")]
    public class ApplyCommand : ICommand
    {
        private readonly ILogger<ApplyCommand> _logger;
        private readonly IYaml _yaml;
        private readonly IKitchen _kitchen;

        public ApplyCommand(
            ILogger<ApplyCommand> logger,
            IYaml yaml,
            IKitchen kitchen)
        {
            _logger = logger;
            _yaml = yaml;
            _kitchen = kitchen;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> ExecuteAsync(
            string planPath,
            CancellationToken cancellationToken)
        {
            planPath = Path.GetFullPath(planPath);
            _logger.LogInformation(
                "Applying plan found at {PlanPath}",
                planPath);

            if (!File.Exists(planPath))
            {
                _logger.LogCritical(
                    "No Bake plan file found at {PlanPath}",
                    planPath);
                return ExitCodes.Apply.PlanFileNotFound;
            }

            var yaml = await File.ReadAllTextAsync(
                planPath,
                cancellationToken);

            var book = await _yaml.DeserializeAsync<Book>(
                yaml,
                cancellationToken);

            var success = await _kitchen.CookAsync(
                new Context(
                    Ingredients.New(
                        book.Ingredients.Version,
                        book.Ingredients.WorkingDirectory)),
                book,
                cancellationToken);

            return success ? 0 : -1;
        }
    }
}
