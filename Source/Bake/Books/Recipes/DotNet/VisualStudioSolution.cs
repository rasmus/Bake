using System.Collections.Generic;
using System.Linq;

namespace Bake.Books.Recipes.DotNet
{
    public class VisualStudioSolution
    {
        public string Path { get; }
        public IReadOnlyCollection<VisualStudioProject> Projects { get; }

        public VisualStudioSolution(
            string path,
            IEnumerable<VisualStudioProject> projects)
        {
            Path = path;
            Projects = projects.ToList();
        }
    }
}
