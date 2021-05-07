using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetBuildCook : Cook<DotNetBuildSolutionRecipe>
    {
        public override string Name => RecipeNames.DotNet.Build;
        
        private readonly IDotNet _dotNet;

        public DotNetBuildCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetBuildSolutionRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetBuildArgument(
                recipe.Path,
                recipe.Configuration,
                recipe.Incremental,
                recipe.Restore,
                recipe.Description,
                recipe.Version);

            return await _dotNet.BuildAsync(
                argument,
                cancellationToken);
        }
    }
}
