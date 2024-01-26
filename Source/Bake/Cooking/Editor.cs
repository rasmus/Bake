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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Ingredients.Gathers;
using Bake.Core;
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking
{
    public class Editor : IEditor
    {
        private readonly ILogger<Editor> _logger;
        private readonly IYaml _yaml;
        private readonly IComposerOrdering _composerOrdering;
        private readonly IReadOnlyCollection<IGather> _gathers;
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            ILogger<Editor> logger,
            IYaml yaml,
            IComposerOrdering composerOrdering,
            IEnumerable<IGather> gathers,
            IEnumerable<IComposer> composers)
        {
            _logger = logger;
            _yaml = yaml;
            _composerOrdering = composerOrdering;
            _gathers = gathers.ToList();
            _composers = composers.ToList();
        }

        public async Task<Book> ComposeAsync(
            Context context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Starting compose for {Convention} run in {WorkingDirectory} with initial version as {Version}",
                context.Ingredients.Convention,
                context.Ingredients.WorkingDirectory,
                context.Ingredients.Version);

            await Task.WhenAll(_gathers.Select(g => g.GatherAsync(context.Ingredients, cancellationToken)));

            var recipes = new List<Recipe>();
            var composers = _composerOrdering.Order(_composers);

            foreach (var composer in composers)
            {
                var composerTypeName = composer.GetType().PrettyPrint();
                _logger.LogInformation(
                    "Executing composer {ComposerType}",
                    composerTypeName);

                var stopWatch = Stopwatch.StartNew();
                var createdRecipes = await composer.ComposeAsync(context, cancellationToken);
                _logger.LogInformation(
                    "Composer {ComposerType} finished after {TotalSeconds}",
                    composerTypeName,
                    stopWatch.Elapsed.TotalSeconds);

                var createdArtifacts = createdRecipes
                    .SelectMany(r => r.Artifacts ?? Artifact.Empty)
                    .ToList();

                recipes.AddRange(createdRecipes);
                context.AddArtifacts(createdArtifacts);
            }

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
    }
}
