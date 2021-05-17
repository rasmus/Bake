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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Ingredients.Gathers;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking
{
    public class Editor : IEditor
    {
        private readonly ILogger<Editor> _logger;
        private readonly IYaml _yaml;
        private readonly IReadOnlyCollection<IGather> _gathers;
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            ILogger<Editor> logger,
            IYaml yaml,
            IEnumerable<IGather> gathers,
            IEnumerable<IComposer> composers)
        {
            _logger = logger;
            _yaml = yaml;
            _gathers = gathers.ToList();
            _composers = composers.ToList();
        }

        public async Task<Book> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            await Task.WhenAll(_gathers.Select(g => g.GatherAsync(context.Ingredients, cancellationToken)));

            var tasks = _composers
                .Select(c => c.ComposeAsync(context, cancellationToken))
                .ToList();

            var recipes = (await Task.WhenAll(tasks))
                .SelectMany(c => c)
                .ToList();

            var book = new Book(
                context.Ingredients,
                recipes);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                var yaml = await _yaml.SerializeAsync(book, cancellationToken);
                _logger.LogTrace("Create the following YAML {Book}", yaml);
            }

            return book;
        }
    }
}
