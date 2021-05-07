using Bake.ValueObjects;

namespace Bake
{
    public interface IContext
    {
        string WorkingDirectory { get; }
        Metadata Metadata { get; }
    }
}
