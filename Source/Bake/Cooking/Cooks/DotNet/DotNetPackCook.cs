using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetPackCook : Cook<DotNetPackProjectRecipe>
    {
        public override string Name => RecipeNames.DotNet.Pack;

        private readonly IDotNet _dotNet;

        public DotNetPackCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetPackProjectRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetPackArgument(
                recipe.Path,
                recipe.Version,
                recipe.Restore,
                recipe.Build,
                recipe.IncludeSymbols,
                recipe.IncludeSource,
                recipe.Configuration);

            return await _dotNet.PackAsync(
                argument,
                cancellationToken);
        }
    }
}
