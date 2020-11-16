namespace Bake.Books.Recipes.DotNet
{
    public class DotNetBuildSolution : Recipe
    {
        public string Path { get; }

        public DotNetBuildSolution(
            string path)
        {
            Path = path;
        }

        public override string ToString()
        {
            return $".NET build: {Path}";
        }
    }
}
