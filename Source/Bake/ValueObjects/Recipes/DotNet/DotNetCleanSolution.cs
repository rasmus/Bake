using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetCleanSolution : DotNetRecipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        public DotNetCleanSolution(
            string path)
        {
            Path = path;
        }

        public override string ToString()
        {
            return $".NET clean {Path}";
        }
    }
}
