namespace Bake.Cooking.Recipes.DotNet
{
    public class DotNetCleanSolution : Recipe
    {
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
