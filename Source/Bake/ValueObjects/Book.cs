using System.Collections.Generic;
using Bake.ValueObjects.Recipes;

namespace Bake.ValueObjects
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
