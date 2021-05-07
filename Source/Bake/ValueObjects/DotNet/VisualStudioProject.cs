namespace Bake.ValueObjects.DotNet
{
    public class VisualStudioProject
    {
        public string Path { get; }
        public CsProj CsProj { get; }
        public string Directory { get; }
        public string Name { get; set; }

        public bool ShouldBePacked => CsProj.IsPackable || CsProj.PackAsTool;
        public bool ShouldBePushed => CsProj.IsPackable || CsProj.PackAsTool;

        public VisualStudioProject(
            string path,
            CsProj csProj)
        {
            Path = path;
            CsProj = csProj;
            Directory = System.IO.Path.GetDirectoryName(path);
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
