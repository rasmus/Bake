using Bake.Core;

namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetBuildArgument : DotNetArgument
    {
        public string Configuration { get; }
        public bool Incremental { get; }
        public bool Restore { get; }
        public string Description { get; }
        public SemVer Version { get; }

        public DotNetBuildArgument(
            string filePath,
            string configuration,
            bool incremental,
            bool restore,
            string description,
            SemVer version)
            : base(filePath)
        {
            Configuration = configuration;
            Incremental = incremental;
            Restore = restore;
            Description = description;
            Version = version;
        }
    }
}
