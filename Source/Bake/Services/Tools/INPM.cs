using System.Threading;
using System.Threading.Tasks;
using Bake.Services.Tools.NPMArguments;

namespace Bake.Services.Tools
{
    public interface INPM
    {
        Task<IToolResult> CIAsync(
            NPMCIArgument argument,
            CancellationToken cancellationToken);
    }
}
