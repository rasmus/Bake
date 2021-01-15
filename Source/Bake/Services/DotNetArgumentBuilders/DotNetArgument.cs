using System.IO;
using Bake.Core;

namespace Bake.Services.DotNetArgumentBuilders
{
    public abstract class DotNetArgument : ArgumentBuilder
    {
        public string FilePath { get; set; }
        public string WorkingDirectory { get; }

        protected DotNetArgument(string filePath)
        {
            FilePath = filePath;
            WorkingDirectory = Path.GetDirectoryName(filePath);
        }
    }
}
