namespace Bake.ValueObjects.DotNet
{
    public class VisualStudioProject
    {
        public string Path { get; }
        public CsProj CsProj { get; }

        public bool ShouldBePacked => CsProj.IsPackable || CsProj.PackAsTool;
        public bool ShouldBePushed => CsProj.IsPackable || CsProj.PackAsTool;

        public VisualStudioProject(
            string path,
            CsProj csProj)
        {
            Path = path;
            CsProj = csProj;
        }
    }
}
