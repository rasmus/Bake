namespace Bake.Services.DotNetArgumentBuilders
{
    public class DotNetTestArgument : DotNetArgument
    {
        public bool Build { get; }
        public bool Restore { get; }
        public string Configuration { get; }

        public DotNetTestArgument(
            string filePath,
            bool build,
            bool restore,
            string configuration)
            : base(filePath)
        {
            Build = build;
            Restore = restore;
            Configuration = configuration;
        }
    }
}
