using Bake.Core;

namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetBuildArgument : DotNetArgument
    {
        public string Configuration { get; }
        public bool Incremental { get; }
        public bool Restore { get; }
        public SemVer Version { get; }

        public DotNetBuildArgument(
            string filePath,
            string configuration,
            bool incremental,
            bool restore,
            SemVer version)
            : base(filePath)
        {
            Configuration = configuration;
            Incremental = incremental;
            Restore = restore;
            Version = version;
        }
    }
}
