using System.Threading;
using System.Threading.Tasks;

namespace Bake.Books
{
    public interface IEditor
    {
        Task ComposeAsync(
            string workingDirectory,
            CancellationToken cancellationToken);
    }
}
