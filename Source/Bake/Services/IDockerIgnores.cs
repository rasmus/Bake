using System.Threading;
using System.Threading.Tasks;

namespace Bake.Services
{
    public interface IDockerIgnores
    {
        Task WriteAsync(
            string directoryPath,
            CancellationToken cancellationToken);
    }
}
