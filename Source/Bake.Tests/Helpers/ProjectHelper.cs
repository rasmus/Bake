using System;
using System.IO;
using System.Linq;

namespace Bake.Tests.Helpers
{
    public static class ProjectHelper
    {
        public static string GetRoot()
        {
            var codeBase = new Uri(typeof(ProjectHelper).Assembly.CodeBase).AbsolutePath;
            var path = Path.GetDirectoryName(codeBase);

            while (!string.IsNullOrEmpty(path))
            {
                var hasReadme = Directory.GetFiles(path)
                    .Any(f => string.Equals(Path.GetFileName(f), "README.md", StringComparison.OrdinalIgnoreCase));
                if (hasReadme)
                {
                    return path;
                }

                path = new DirectoryInfo(path).Parent?.FullName;
            }

            throw new InvalidOperationException($"Could find project from '{codeBase}'");
        }
    }
}
