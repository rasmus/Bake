using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects.Recipes;

namespace Bake.Cooking
{
    public interface IComposer
    {
        Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken);
    }
}
