namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetRestoreArgument : DotNetArgument
    {
        public DotNetRestoreArgument(
            string solutionPath)
            : base(solutionPath)
        {
        }
    }
}
