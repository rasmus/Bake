﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Ingredients.Gathers;
using Bake.ValueObjects;

namespace Bake.Cooking
{
    public class Editor : IEditor
    {
        private readonly IReadOnlyCollection<IGather> _gathers;
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            IEnumerable<IGather> gathers,
            IEnumerable<IComposer> composers)
        {
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

            return book;
        }
    }
}
