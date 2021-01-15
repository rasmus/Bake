using Bake.Core;

namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetPackArgument : DotNetArgument
    {
        public SemVer Version { get; }
        public bool Restore { get; }
        public bool Build { get; }
        public bool IncludeSymbols { get; }
        public bool IncludeSource { get; }
        public string Configuration { get; }

        public DotNetPackArgument(
            string filePath,
            SemVer version,
            bool restore,
            bool build,
            bool includeSymbols,
            bool includeSource,
            string configuration)
            : base(filePath)
        {
            Version = version;
            Restore = restore;
            Build = build;
            IncludeSymbols = includeSymbols;
            IncludeSource = includeSource;
            Configuration = configuration;
        }
    }
}
