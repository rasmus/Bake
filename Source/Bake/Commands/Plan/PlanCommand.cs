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

using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking;
using Bake.Core;
using Bake.ValueObjects;
using Bake.ValueObjects.Destinations;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Bake.Commands.Plan
{
    [CommandVerb("plan")]
    public class PlanCommand : ICommand
    {
        private readonly ILogger<PlanCommand> _logger;
        private readonly IEditor _editor;
        private readonly IYaml _yaml;
        private readonly ILogCollector _logCollector;

        public PlanCommand(
            ILogger<PlanCommand> logger,
            IEditor editor,
            IYaml yaml,
            ILogCollector logCollector)
        {
            _logger = logger;
            _editor = editor;
            _yaml = yaml;
            _logCollector = logCollector;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> ExecuteAsync(
            SemVer buildVersion,
            string planPath,
            CancellationToken cancellationToken,
            bool force = false,
            Convention convention = Convention.Default,
            Destination[] destination = null,
            LogEventLevel logLevel = LogEventLevel.Information)
        {
            _logCollector.LogLevel = logLevel;

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

            if (System.IO.File.Exists(planPath))
            {
                if (force)
                {
                    _logger.LogWarning(
                        "File already exists at {PlanPath}, but asked to force the operation so deleting it",
                        planPath);
                    System.IO.File.Delete(planPath);
                }
                else
                {
                    _logger.LogCritical(
                        "File already exists at {PlanPath}",
                        planPath);
                    return ExitCodes.Plan.PlanFileAlreadyExists;
                }
            }

            var content = Context.New(
                Ingredients.New(
                    buildVersion,
                    Directory.GetCurrentDirectory(),
                    convention));
            content.Ingredients.Destinations.AddRange(destination ?? Enumerable.Empty<Destination>());

            var book = await _editor.ComposeAsync(
                content,
                cancellationToken);

            var yaml = await _yaml.SerializeAsync(
                book,
                cancellationToken);

            _logger.LogInformation(
                "Writing plan to {PlanPath}",
                planPath);
            await System.IO.File.WriteAllTextAsync(
                planPath,
                yaml,
                Encoding.UTF8,
                cancellationToken);

            return 0;
        }
    }
}
