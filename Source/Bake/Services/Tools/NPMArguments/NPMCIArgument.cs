namespace Bake.Services.Tools.NPMArguments
{
    public class NPMCIArgument
    {
        public string WorkingDirectory { get; }

        public NPMCIArgument(
            string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }
    }
}
