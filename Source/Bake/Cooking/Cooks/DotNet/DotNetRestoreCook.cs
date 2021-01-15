using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetRestoreCook : Cook<DotNetRestoreSolution>
    {
        public override string Name => "dotnet-restore";
        
        private readonly IDotNet _dotNet;

        public DotNetRestoreCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetRestoreSolution recipe,
            CancellationToken cancellationToken)
        {
            if (recipe.ClearLocalHttpCache)
            {
                var success = await _dotNet.ClearNuGetLocalsAsync(
                    cancellationToken);
                if (!success)
                {
                    return false;
                }
            }

            return await _dotNet.RestoreAsync(
                new DotNetRestoreArgument(
                    recipe.Path),
                cancellationToken);
        }
    }
}
