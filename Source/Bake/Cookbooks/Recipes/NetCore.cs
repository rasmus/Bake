using System.Threading;
using System.Threading.Tasks;
using Bake.Services;

namespace Bake.Cookbooks.Recipes
{
    public class NetCore : IRecipe
    {
        private readonly IDotNet _dotNet;

        public string Name => "net-core";

        public NetCore(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        public async Task BakeAsync(
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            await _dotNet.CleanAsync(
                workingDirectory,
                cancellationToken);

            await _dotNet.RestoreAsync(
                workingDirectory,
                cancellationToken);

            await _dotNet.BuildAsync(
                workingDirectory,
                cancellationToken);
        }
    }
}
