using System.Collections.Generic;
using Bake.ValueObjects.Recipes;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Book
    {
        [YamlMember(Alias = "metadata")]
        public Metadata Metadata { get; }

        [YamlMember(Alias = "recipes")]
        public IReadOnlyCollection<Recipe> Recipes { get; }

        public Book(
            Metadata metadata,
            IReadOnlyCollection<Recipe> recipes)
        {
            Metadata = metadata;
            Recipes = recipes;
        }
    }
}
