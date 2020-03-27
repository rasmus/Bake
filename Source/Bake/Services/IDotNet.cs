using System.Threading;
using System.Threading.Tasks;

namespace Bake.Services
{
    public interface IDotNet
    {
        Task CleanAsync(
            string workingDirectory,
            CancellationToken cancellationToken);

        Task RestoreAsync(
            string workingDirectory,
            CancellationToken cancellationToken);

        Task BuildAsync(
            string workingDirectory,
            CancellationToken cancellationToken);
    }
}
