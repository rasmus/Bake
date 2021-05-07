using Bake.Core;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetPackProjectRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "restore")]
        public bool Restore { get; }

        [YamlMember(Alias = "build")]
        public bool Build { get; }

        [YamlMember(Alias = "include-symbols")]
        public bool IncludeSymbols { get; }

        [YamlMember(Alias = "include-source")]
        public bool IncludeSource { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        [YamlMember(Alias = "version")]
        public SemVer Version { get; }

        public DotNetPackProjectRecipe(
            string path,
            bool restore,
            bool build,
            bool includeSymbols,
            bool includeSource,
            string configuration,
            SemVer version)
            : base(RecipeNames.DotNet.Pack)
        {
            Path = path;
            Restore = restore;
            Build = build;
            IncludeSymbols = includeSymbols;
            IncludeSource = includeSource;
            Configuration = configuration;
            Version = version;
        }

        public override string ToString()
        {
            return $".NET pack {Path}";
        }
    }
}
