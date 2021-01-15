namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetCleanArgument : DotNetArgument
    {
        public string Configuration { get; }

        public DotNetCleanArgument(
            string filePath,
            string configuration)
            : base(filePath)
        {
            Configuration = configuration;
        }
    }
}
