using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetPublishArgument : DotNetArgument
    {
        public bool PublishSingleFile { get; }
        public bool SelfContained { get; }
        public DotNetTargetRuntime Runtime { get; }

        public DotNetPublishArgument(
            string filePath,
            bool publishSingleFile,
            bool selfContained,
            DotNetTargetRuntime runtime)
            : base(filePath)
        {
            PublishSingleFile = publishSingleFile;
            SelfContained = selfContained;
            Runtime = runtime;
        }
    }
}
