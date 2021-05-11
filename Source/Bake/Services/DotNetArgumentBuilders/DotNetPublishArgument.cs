using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetPublishArgument
    {
        public string Path { get; }
        public string WorkingDirectory { get; set; }
        public bool PublishSingleFile { get; }
        public bool SelfContained { get; }
        public DotNetTargetRuntime Runtime { get; }

        public DotNetPublishArgument(
            string path,
            bool publishSingleFile,
            bool selfContained,
            DotNetTargetRuntime runtime)
        {
            Path = path;
            WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            PublishSingleFile = publishSingleFile;
            SelfContained = selfContained;
            Runtime = runtime;
        }
    }
}
