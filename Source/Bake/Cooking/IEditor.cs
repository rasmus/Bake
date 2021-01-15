using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects;

namespace Bake.Cooking
{
    public interface IEditor
    {
        Task<Book> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken);
    }
}
