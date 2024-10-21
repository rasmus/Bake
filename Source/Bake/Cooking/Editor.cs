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

using System.Diagnostics;
using Bake.Cooking.Ingredients.Gathers;
using Bake.Core;
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Recipes;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking
{
    public class Editor : IEditor
    {
        private readonly ILogger<Editor> _logger;
        private readonly IYaml _yaml;
        private readonly IComposerOrdering _composerOrdering;
        private readonly IDefaults _defaults;
        private readonly IReadOnlyCollection<IGather> _gathers;
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            ILogger<Editor> logger,
            IYaml yaml,
            IComposerOrdering composerOrdering,
            IDefaults defaults,
            IEnumerable<IGather> gathers,
            IEnumerable<IComposer> composers)
        {
            _logger = logger;
            _yaml = yaml;
            _composerOrdering = composerOrdering;
            _defaults = defaults;
            _gathers = gathers.ToList();
            _composers = composers.ToList();
        }

        public async Task<Book> ComposeAsync(
            Context context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Starting ingredients gathering in {WorkingDirectory}",
                context.Ingredients.WorkingDirectory);
            await ExecuteGatherersAsync(context, cancellationToken);

            _logger.LogInformation(
                "Starting compose for {Convention} run in {WorkingDirectory} with initial version as {Version}",
                context.Ingredients.Convention,
                context.Ingredients.WorkingDirectory,
                context.Ingredients.Version);
            var recipes = await ExecuteComposersAsync(context, cancellationToken);

            var book = new Book(
                context.Ingredients,
                recipes.ToArray());

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                var yaml = await _yaml.SerializeAsync(book, cancellationToken);
                _logger.LogTrace("Create the following YAML {Book}", yaml);
            }

            return book;
        }

        private async Task ExecuteGatherersAsync(Context context, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_defaults.BakeIngredientsGatherTimeout);

            try
            {
                await Task.WhenAll(_gathers.Select(g => g.GatherAsync(context.Ingredients, timeout.Token)));
            }
            catch (Exception e) when (e is TaskCanceledException or OperationCanceledException && timeout.IsCancellationRequested)
            {
                Console.WriteLine("Gathering repository meta data took too long! Aborting :(");
                throw;
            }
        }

        private async Task<List<Recipe>> ExecuteComposersAsync(Context context, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_defaults.BakeComposeTimeout);

            var recipes = new List<Recipe>();
            var composers = _composerOrdering.Order(_composers);

            foreach (var composer in composers)
            {
                var composerTypeName = composer.GetType().PrettyPrint();
                _logger.LogInformation(
                    "Executing composer {ComposerType}",
                    composerTypeName);

                var stopWatch = Stopwatch.StartNew();
                var createdRecipes = await composer.ComposeAsync(context, timeout.Token);
                _logger.LogInformation(
                    "Composer {ComposerType} finished after {TotalSeconds}",
                    composerTypeName,
                    stopWatch.Elapsed.TotalSeconds);

                var createdArtifacts = createdRecipes
                    .SelectMany(r => r.Artifacts)
                    .ToList();

                recipes.AddRange(createdRecipes);
                context.AddArtifacts(createdArtifacts);
            }

            return recipes;
        }
    }
}
