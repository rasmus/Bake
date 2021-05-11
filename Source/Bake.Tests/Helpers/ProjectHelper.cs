// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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




