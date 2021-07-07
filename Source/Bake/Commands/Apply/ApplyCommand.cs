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
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Bake.Commands.Apply
{
    [CommandVerb("apply")]
    public class ApplyCommand : ICommand
    {
        private readonly ILogger<ApplyCommand> _logger;
        private readonly IYaml _yaml;
        private readonly IKitchen _kitchen;
        private readonly ILogCollector _logCollector;

        public ApplyCommand(
            ILogger<ApplyCommand> logger,
            IYaml yaml,
            IKitchen kitchen,
            ILogCollector logCollector)
        {
            _logger = logger;
            _yaml = yaml;
            _kitchen = kitchen;
            _logCollector = logCollector;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> ExecuteAsync(
            string planPath,
            CancellationToken cancellationToken,
            LogEventLevel logLevel = LogEventLevel.Information)
        {
            _logCollector.LogLevel = logLevel;

            planPath = Path.GetFullPath(planPath);
            _logger.LogInformation(
                "Applying plan found at {PlanPath}",
                planPath);

            if (!System.IO.File.Exists(planPath))
            {
                _logger.LogCritical(
                    "No Bake plan file found at {PlanPath}",
                    planPath);
                return ExitCodes.Apply.PlanFileNotFound;
            }

            var yaml = await System.IO.File.ReadAllTextAsync(
                planPath,
                cancellationToken);

            var book = await _yaml.DeserializeAsync<Book>(
                yaml,
                cancellationToken);

            var context = Context.New(
                Ingredients.New(
                    book.Ingredients.Version,
                    book.Ingredients.WorkingDirectory));

            var success = await _kitchen.CookAsync(
                context,
                book,
                cancellationToken);

            return success ? 0 : -1;
        }
    }
}
