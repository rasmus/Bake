using System.Collections.Generic;
using System.Linq;

namespace Bake.ValueObjects.DotNet
{
    public class VisualStudioSolution
    {
        public string Path { get; }
        public string Name { get; }
        public IReadOnlyCollection<VisualStudioProject> Projects { get; }

        public VisualStudioSolution(
            string path,
            IEnumerable<VisualStudioProject> projects)
        {
            Path = path;
            Projects = projects.ToList();
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
