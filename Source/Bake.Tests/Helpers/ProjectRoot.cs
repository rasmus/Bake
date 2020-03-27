using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bake.Tests.Helpers
{
    public static class ProjectRoot
    {
        public static string Get()
        {
            var codeBase = GetCodeBase(typeof(ProjectRoot).GetTypeInfo().Assembly);
            return GetParentDirectories(codeBase).First(IsProjectRoot);
        }

        private static IEnumerable<string> GetParentDirectories(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException($"Directory '{path}' does not exist!");

            var parent = Directory.GetParent(path);
            while (parent != null)
            {
                yield return parent.FullName;
                parent = parent.Parent;
            }
        }

        private static bool IsProjectRoot(string path)
        {
            return Directory
                .GetFiles(path)
                .Any(f => f.EndsWith("README.md", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetCodeBase(Assembly assembly, bool includeFileName = false)
        {
            var codebase = assembly.CodeBase;
            var uri = new UriBuilder(codebase);
            var path = Path.GetFullPath(Uri.UnescapeDataString(uri.Path));
            var codeBase = includeFileName ?
                path :
                Path.GetDirectoryName(path);
            return codeBase;
        }
    }
}
