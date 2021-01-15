using System.IO;
using Bake.Core;

namespace Bake.Services.DotNetArgumentBuilders
{
    public abstract class DotNetArgument : ArgumentBuilder
    {
        public string SolutionPath { get; set; }
        public string WorkingDirectory { get; }

        protected DotNetArgument(string solutionPath)
        {
            SolutionPath = solutionPath;
            WorkingDirectory = Path.GetDirectoryName(solutionPath);
        }
    }
}
