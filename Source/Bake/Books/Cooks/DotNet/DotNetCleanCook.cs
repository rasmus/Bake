using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Recipes.DotNet;

namespace Bake.Books.Cooks.DotNet
{
    public class DotNetCleanCook : Cook<DotNetCleanSolution>
    {
        public override void Initialize(ICookInitializer cookInitializer)
        {
            base.Initialize(cookInitializer);
        }

        protected override Task CookAsync(
            IContext context,
            DotNetCleanSolution recipe,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
