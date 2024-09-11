// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using System.Text;
using Bake.Cooking;
using Bake.Core;
using Bake.ValueObjects;
using Bake.ValueObjects.Destinations;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Bake.Commands.Run
{
    [Command(
        "run",
        "Analyzes the current directory, builds a plan and then executes the plan")]
    public class RunCommand : ICommand
    {
        private readonly ILogger<RunCommand> _logger;
        private readonly IEditor _editor;
        private readonly IKitchen _kitchen;
        private readonly ILogCollector _logCollector;
        private readonly IYaml _yaml;

        public RunCommand(
            ILogger<RunCommand> logger,
            IEditor editor,
            IKitchen kitchen,
            ILogCollector logCollector,
            IYaml yaml)
        {
            _logger = logger;
            _editor = editor;
            _kitchen = kitchen;
            _logCollector = logCollector;
            _yaml = yaml;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> ExecuteAsync(
            SemVer buildVersion,
            CancellationToken cancellationToken,
            Convention convention = Convention.Default,
            Destination[]? destination = null,
            LogEventLevel logLevel = LogEventLevel.Information,
            bool printPlan = true,
            bool pushContainerLatestTag = false,
            bool signArtifacts = false,
            Platform[]? targetPlatform = null)
        {
            _logCollector.LogLevel = logLevel;

            var content = Context.New(
                Ingredients.New(
                    buildVersion,
                    Directory.GetCurrentDirectory(),
                    targetPlatform,
                    convention,
                    pushContainerLatestTag,
                    signArtifacts));

            content.Ingredients.Destinations.AddRange(destination ?? Enumerable.Empty<Destination>());

            var book = await _editor.ComposeAsync(
                content,
                cancellationToken);

            if (!book.Recipes.Any())
            {
                _logger.LogCritical("No recipes created!");
                return ExitCodes.Core.NoRecipes;
            }

            if (printPlan)
            {
                var plan = new StringBuilder()
                    .AppendLine(new string('=', 40))
                    .AppendLine(await _yaml.SerializeAsync(book, cancellationToken))
                    .AppendLine(new string('=', 40))
                    .ToString();
                Console.WriteLine(plan);
            }

            var success = await _kitchen.CookAsync(
                content,
                book,
                cancellationToken);

            return success
                ? 0
                : ExitCodes.Core.CookingFailed;
        }
    }
}
