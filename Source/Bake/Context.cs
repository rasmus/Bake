using Bake.Core;

namespace Bake
{
    public class Context : IContext
    {
        public SemVer Version { get; }
        public string WorkingDirectory { get; }

        public Context(
            SemVer version,
            string workingDirectory)
        {
            Version = version;
            WorkingDirectory = workingDirectory;
        }
    }
}
