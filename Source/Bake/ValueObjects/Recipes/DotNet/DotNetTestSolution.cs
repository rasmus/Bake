using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetTestSolution : DotNetRecipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "build")]
        public bool Build { get; }

        [YamlMember(Alias = "restore")]
        public bool Restore { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        public DotNetTestSolution(
            string path,
            bool build,
            bool restore,
            string configuration)
            : base("dotnet-test")
        {
            Path = path;
            Build = build;
            Restore = restore;
            Configuration = configuration;
        }
    }
}
