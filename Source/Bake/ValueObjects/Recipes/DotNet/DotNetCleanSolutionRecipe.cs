using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetCleanSolutionRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        public DotNetCleanSolutionRecipe(
            string path,
            string configuration)
            : base(RecipeNames.DotNet.Clean)
        {
            Path = path;
            Configuration = configuration;
        }

        public override string ToString()
        {
            return $".NET clean {Path}";
        }
    }
}
