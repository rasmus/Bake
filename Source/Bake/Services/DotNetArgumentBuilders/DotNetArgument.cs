using Bake.Core;

namespace Bake.Services.DotNetArgumentBuilders
{
    public abstract class DotNetArgument : ArgumentBuilder
    {
        public string WorkingDirectory { get; }

        protected DotNetArgument(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }
    }
}