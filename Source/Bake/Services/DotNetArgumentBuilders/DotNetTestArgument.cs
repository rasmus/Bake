namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetTestArgument : DotNetArgument
    {
        public bool Build { get; }
        public bool Restore { get; }

        public DotNetTestArgument(
            string solutionPath,
            bool build,
            bool restore)
            : base(solutionPath)
        {
            Build = build;
            Restore = restore;
        }
    }
}
