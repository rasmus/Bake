using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetTestCook : Cook<DotNetTestSolution>
    {
        public override string Name => "dotnet-test";

        private readonly IDotNet _dotNet;

        public DotNetTestCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetTestSolution recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetTestArgument(
                recipe.Path,
                recipe.Build,
                recipe.Restore);

            return await _dotNet.TestAsync(
                argument,
                cancellationToken);
        }
    }
}
