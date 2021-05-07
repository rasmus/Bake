using Bake.ValueObjects;

namespace Bake
{
    public class Context : IContext
    {
        public Metadata Metadata { get; }
        public string WorkingDirectory { get; }

        public Context(
            Metadata metadata,
            string workingDirectory)
        {
            Metadata = metadata;
            WorkingDirectory = workingDirectory;
        }
    }
}
