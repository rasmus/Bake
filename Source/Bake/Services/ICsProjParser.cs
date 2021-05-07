using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects.DotNet;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Services
{
    public interface ICsProjParser
    {
        Task<CsProj> ParseAsync(
            string path,
            CancellationToken cancellationToken);
    }
}
