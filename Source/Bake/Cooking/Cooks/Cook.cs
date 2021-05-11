using System;
using System.Threading;
using System.Threading.Tasks;
using Bake.Extensions;
using Bake.ValueObjects.Recipes;

namespace Bake.Cooking.Cooks
{
    public abstract class Cook<T> : ICook
        where T : Recipe
    {
        public abstract string RecipeName { get; }
        public Type CanCook { get; } = typeof(T);

        public Task<bool> CookAsync(
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

        protected abstract Task<bool> CookAsync(
            IContext context,
            T recipe,
            CancellationToken cancellationToken);
    }
}
