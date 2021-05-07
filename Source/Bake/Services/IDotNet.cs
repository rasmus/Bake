using System.Threading;
using System.Threading.Tasks;
using Bake.Services.DotNetArgumentBuilders;

namespace Bake.Services
{
    public interface IDotNet
    {
        Task<bool> CleanAsync(
            DotNetCleanArgument argument,
            CancellationToken cancellationToken);

        Task<bool> BuildAsync(
            DotNetBuildArgument argument,
            CancellationToken cancellationToken);

        Task<bool> RestoreAsync(
            DotNetRestoreArgument argument,
            CancellationToken cancellationToken);

        Task<bool> TestAsync(
            DotNetTestArgument argument,
            CancellationToken cancellationToken);

        Task<bool> ClearNuGetLocalsAsync(
            CancellationToken cancellationToken);

        Task<bool> PackAsync(
            DotNetPackArgument argument,
            CancellationToken cancellationToken);

        Task<bool> NuGetPushAsync(
            DotNetNuGetPushArgument argument,
            CancellationToken cancellationToken);
    }
}
