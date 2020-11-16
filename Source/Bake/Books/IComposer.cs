using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Recipes;

namespace Bake.Books
{
    public interface IComposer
    {
        Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            string workingDirectory,
            CancellationToken cancellationToken);
    }
}
