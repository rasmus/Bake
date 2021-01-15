using System;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Recipes;

namespace Bake.Cooking.Cooks
{
    public interface ICook
    {
        string Name { get; }

        Type CanCook { get; }

        Task CookAsync(
            IContext context,
            Recipe recipe,
            CancellationToken cancellationToken);
    }
}
