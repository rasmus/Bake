using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetRestoreSolutionRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "clear-local-http-cache")]
        public bool ClearLocalHttpCache { get; }

        public DotNetRestoreSolutionRecipe(
            string path,
            bool clearLocalHttpCache)
            : base(RecipeNames.DotNet.Restore)
        {
            Path = path;
            ClearLocalHttpCache = clearLocalHttpCache;
        }
    }
}
