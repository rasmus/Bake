using System;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Recipes;
using Bake.Extensions;

namespace Bake.Cooking.Cooks
{
    public abstract class Cook<T> : ICook
        where T : Recipe
    {
        public abstract string Name { get; }
        public Type CanCook { get; } = typeof(T);

        public Task CookAsync(
            IContext context,
            Recipe recipe,
            CancellationToken cancellationToken)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (!(recipe is T myRecipe))
            {
                throw new ArgumentException(
                    $"I don't know how to cook a '{recipe.GetType().PrettyPrint()}'",
                    nameof(recipe));
            }

            return CookAsync(context, myRecipe, cancellationToken);
        }

        protected abstract Task CookAsync(
            IContext context,
            T recipe,
            CancellationToken cancellationToken);
    }
}
