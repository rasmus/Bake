using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetTestSolutionRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "build")]
        public bool Build { get; }

        [YamlMember(Alias = "restore")]
        public bool Restore { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        public DotNetTestSolutionRecipe(
            string path,
            bool build,
            bool restore,
            string configuration)
            : base(RecipeNames.DotNet.Test)
        {
            Path = path;
            Build = build;
            Restore = restore;
            Configuration = configuration;
        }
    }
}
