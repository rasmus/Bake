using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetPublishCook : Cook<DotNetPublishRecipe>
    {
        private readonly IDotNet _dotNet;
        public override string Name => RecipeNames.DotNet.Publish;

        public DotNetPublishCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override Task<bool> CookAsync(
            IContext context,
            DotNetPublishRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetPublishArgument(
                recipe.Path,
                recipe.PublishSingleFile,
                recipe.SelfContained,
                recipe.Runtime);

            return _dotNet.PublishAsync(
                argument,
                cancellationToken);
        }
    }
}
