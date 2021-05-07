namespace Bake.ValueObjects.Recipes.DotNet
{
    public class DotNetNuGetPushRecipe : Recipe
    {
        public string FilePath { get; }
        public string ApiKey { get; }
        public string Source { get; }

        public DotNetNuGetPushRecipe(
            string filePath,
            string apiKey,
            string source = null)
            : base(RecipeNames.DotNet.NuGetPush)
        {
            FilePath = filePath;
            ApiKey = apiKey;
            Source = source;
        }
    }
}
