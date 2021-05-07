using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetNuGetPushCook : Cook<DotNetNuGetPushRecipe>
    {
        public override string Name => RecipeNames.DotNet.NuGetPush;

        private readonly IDotNet _dotNet;

        public DotNetNuGetPushCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetNuGetPushRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetNuGetPushArgument(
                recipe.ApiKey,
                recipe.Source,
                recipe.FilePath);

            return await _dotNet.NuGetPushAsync(
                argument,
                cancellationToken);
        }
    }
}
