namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetNuGetPushArgument : DotNetArgument
    {
        public string ApiKey { get; }
        public string Source { get; }

        public DotNetNuGetPushArgument(
            string apiKey,
            string source,
            string filePath)
            : base(filePath)
        {
            ApiKey = apiKey;
            Source = source;
        }
    }
}
