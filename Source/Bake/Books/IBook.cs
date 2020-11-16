using System.Threading;
using System.Threading.Tasks;

namespace Bake.Books
{
    public interface IBook
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
