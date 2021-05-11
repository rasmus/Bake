using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetNuGetPushCook : Cook<DotNetNuGetPushRecipe>
    {
        public override string RecipeName => RecipeNames.DotNet.NuGetPush;

        private readonly ILogger<DotNetNuGetPushCook> _logger;
        private readonly IDotNet _dotNet;

        public DotNetNuGetPushCook(
            ILogger<DotNetNuGetPushCook> logger,
            IDotNet dotNet)
        {
            _logger = logger;
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetNuGetPushRecipe recipe,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(recipe.FilePath))
            {
                _logger.LogCritical(
                    "NuGet package not found at {NuGetPackagePath}",
                    recipe.FilePath);
                return false;
            }

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
