using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetRestoreSolution : DotNetRecipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "clear-local-http-cache")]
        public bool ClearLocalHttpCache { get; }

        public DotNetRestoreSolution(
            string path,
            bool clearLocalHttpCache)
        {
            Path = path;
            ClearLocalHttpCache = clearLocalHttpCache;
        }
    }
}
