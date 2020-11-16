using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Recipes;

namespace Bake.Books
{
    public interface IComposer
    {
        Task<Recipe> ComposeAsync(
            string workingDirectory,
            CancellationToken cancellationToken);
    }
}
