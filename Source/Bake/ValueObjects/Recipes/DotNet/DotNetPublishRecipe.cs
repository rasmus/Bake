using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetPublishRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "publish-single-file")]
        public bool PublishSingleFile { get; }

        [YamlMember(Alias = "self-contained")]
        public bool SelfContained { get; }

        [YamlMember(Alias = "runtime")]
        public DotNetTargetRuntime Runtime { get; }

        public DotNetPublishRecipe(
            string path,
            bool publishSingleFile,
            bool selfContained,
            DotNetTargetRuntime runtime)
            : base(RecipeNames.DotNet.Publish)
        {
            Path = path;
            PublishSingleFile = publishSingleFile;
            SelfContained = selfContained;
            Runtime = runtime;
        }
    }
}
