using System.Collections.Generic;
using Bake.Cooking.Recipes;

namespace Bake.Cooking
{
    public class Book
    {
        public IReadOnlyCollection<Recipe> Recipes { get; }

        public Book(
            IReadOnlyCollection<Recipe> recipes)
        {
            Recipes = recipes;
        }
    }
}
