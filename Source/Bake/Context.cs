using Bake.Core;

namespace Bake
{
    public class Context : IContext
    {
        public SemVer Version { get; }

        public Context(SemVer version)
        {
            Version = version;
        }
    }
}
