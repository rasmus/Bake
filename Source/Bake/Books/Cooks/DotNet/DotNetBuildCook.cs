using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Recipes.DotNet;

namespace Bake.Books.Cooks.DotNet
{
    public class DotNetBuildCook : Cook<DotNetBuildSolution>
    {
        public override void Initialize(ICookInitializer cookInitializer)
        {
            base.Initialize(cookInitializer);

            cookInitializer
                .DependOn<DotNetCleanCook>();
        }

        protected override Task CookAsync(
            IContext context,
            DotNetBuildSolution recipe,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
