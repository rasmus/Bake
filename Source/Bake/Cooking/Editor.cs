using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects;

namespace Bake.Cooking
{
    public class Editor : IEditor
    {
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            IEnumerable<IComposer> composers)
        {
            _composers = composers.ToList();
        }

        public async Task<Book> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var tasks = _composers
                .Select(c => c.ComposeAsync(context, cancellationToken))
                .ToList();

            var recipes = (await Task.WhenAll(tasks))
                .SelectMany(c => c)
                .ToList();

            return new Book(recipes);
        }
    }
}
