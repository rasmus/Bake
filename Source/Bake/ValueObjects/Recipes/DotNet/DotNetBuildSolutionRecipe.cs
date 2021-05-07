using Bake.Core;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetBuildSolutionRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        [YamlMember(Alias = "restore")]
        public bool Restore { get; }

        [YamlMember(Alias = "incremental")]
        public bool Incremental { get; }

        [YamlMember(Alias = "version")]
        public SemVer Version { get; }

        [YamlMember(Alias = "description")]
        public string Description { get; }

        public DotNetBuildSolutionRecipe(
            string path,
            string configuration,
            bool restore,
            bool incremental,
            string description,
            SemVer version)
            :base(RecipeNames.DotNet.Build)
        {
            Path = path;
            Configuration = configuration;
            Restore = restore;
            Incremental = incremental;
            Description = description;
            Version = version;
        }

        public override string ToString()
        {
            return $".NET build: {Path}";
        }
    }
}
