using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Cooks;

namespace Bake.Books
{
    public class Book : IBook
    {
        private readonly IReadOnlyCollection<ICook> _recipes;

        public Book(
            IEnumerable<ICook> recipes)
        {
            _recipes = recipes.ToList();
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
