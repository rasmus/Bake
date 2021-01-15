using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetCleanSolution : DotNetRecipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        public DotNetCleanSolution(
            string path,
            string configuration)
            : base("dotnet-clean")
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
