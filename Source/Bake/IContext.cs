using Bake.Core;

namespace Bake
{
    public interface IContext
    {
        SemVer Version { get; }
    }
}
