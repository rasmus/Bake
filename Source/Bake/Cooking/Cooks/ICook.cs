using System;
using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects.Recipes;

namespace Bake.Cooking.Cooks
{
    public interface ICook
    {
        string RecipeName { get; }

        Type CanCook { get; }

        Task<bool> CookAsync(
            IContext context,
            Recipe recipe,
            CancellationToken cancellationToken);
    }
}
