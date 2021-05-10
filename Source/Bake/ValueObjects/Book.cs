using System;
using System.Collections.Generic;
using Bake.ValueObjects.Recipes;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Book
    {
        [YamlMember(Alias = "metadata")]
        public Ingredients Ingredients { get; [Obsolete] set; }

        [YamlMember(Alias = "recipes")]
        public IReadOnlyCollection<Recipe> Recipes { get; [Obsolete] set; }

        public Book(
            Ingredients ingredients,
            IReadOnlyCollection<Recipe> recipes)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Ingredients = ingredients;
            Recipes = recipes;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
