using System.Threading;
using System.Threading.Tasks;
using Bake.Services.DotNetArgumentBuilders;

namespace Bake.Services
{
    public interface IDotNet
    {
        Task BuildAsync(string workingDirectory,
            DotNetBuildArgument argument,
            CancellationToken cancellationToken);
    }
}
