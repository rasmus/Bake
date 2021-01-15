namespace Bake.ValueObjects.Recipes.DotNet
{
    public class CsProj
    {
        public bool PackAsTool { get; }
        public string ToolCommandName { get; }
        public bool IsPackable { get; }
        public bool IsPublishable { get; }

        public CsProj(
            bool packAsTool,
            string toolCommandName,
            bool isPackable,
            bool isPublishable)
        {
            PackAsTool = packAsTool;
            ToolCommandName = toolCommandName;
            IsPackable = isPackable;
            IsPublishable = isPublishable;
        }
    }
}
