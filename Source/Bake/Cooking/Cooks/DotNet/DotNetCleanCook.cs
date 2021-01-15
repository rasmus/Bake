using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetCleanCook : Cook<DotNetCleanSolution>
    {
        public override string Name => "dotnet-clean";

        private readonly IDotNet _dotNet;

        public DotNetCleanCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetCleanSolution recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetCleanArgument(
                recipe.Path,
                recipe.Configuration);

            return await _dotNet.CleanAsync(
                argument,
                cancellationToken);
        }
    }
}
