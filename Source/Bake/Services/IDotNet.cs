using System.Threading;
using System.Threading.Tasks;
using Bake.Services.DotNetArgumentBuilders;

namespace Bake.Services
{
    public interface IDotNet
    {
        Task CleanAsync(
            DotNetCleanArgument argument,
            CancellationToken cancellationToken);

        Task BuildAsync(
            DotNetBuildArgument argument,
            CancellationToken cancellationToken);
    }
}
