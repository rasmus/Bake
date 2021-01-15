using System.Threading;
using System.Threading.Tasks;

namespace Bake.Cooking
{
    public interface IEditor
    {
        Task<Book> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken);
    }
}
