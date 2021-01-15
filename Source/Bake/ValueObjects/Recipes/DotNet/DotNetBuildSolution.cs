using Bake.Core;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetBuildSolution : DotNetRecipe
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

        public DotNetBuildSolution(
            string path,
            string configuration,
            bool restore,
            bool incremental,
            SemVer version)
            :base("dotnet-build")
        {
            Path = path;
            Configuration = configuration;
            Restore = restore;
            Incremental = incremental;
            Version = version;
        }

        public override string ToString()
        {
            return $".NET build: {Path}";
        }
    }
}
