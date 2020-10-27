using System.Threading;
using System.Threading.Tasks;

namespace Bake.Services
{
    public interface IDotNet
    {
        Task BuildAsync(
            string workingDirectory,
            CancellationToken cancellationToken);
    }
}
