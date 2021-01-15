using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes
{
    public abstract class Recipe
    {
        [YamlMember(Alias = "name", Order = -1)]
        public string Name { get; }

        protected Recipe(string name)
        {
            Name = name;
        }
    }
}
