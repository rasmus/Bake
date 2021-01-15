using System.Collections.Generic;
using Bake.ValueObjects.Recipes;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Book
    {
        [YamlMember(Alias = "recipes")]
        public IReadOnlyCollection<Recipe> Recipes { get; }

        public Book(
            IReadOnlyCollection<Recipe> recipes)
        {
            Recipes = recipes;
        }
    }
}
