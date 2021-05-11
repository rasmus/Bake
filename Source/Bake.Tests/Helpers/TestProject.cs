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

using System.IO;
using System.Threading.Tasks;
using Bake.Core;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public abstract class TestProject : TestIt
    {
        protected string ProjectName { get; }
        protected string WorkingDirectory => _folder.Path;
        private string _previousCurrentDirectory;

        private Folder _folder;

        protected TestProject(
            string projectName)
        {
            ProjectName = projectName;
        }

        [SetUp]
        public void SetUpTestProject()
        {
            _folder = Folder.New;

            GitHelper.Create(_folder.Path);

            DirectoryCopy(
                Path.Combine(
                    ProjectHelper.GetRoot(),
                    "TestProjects",
                    ProjectName),
                _folder.Path);

            _previousCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(WorkingDirectory);
        }

        [TearDown]
        public async Task TearDownTestProject()
        {
            Directory.SetCurrentDirectory(_previousCurrentDirectory);
            await _folder.DisposeAsync();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            var dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (var subDirectory in dirs)
            {
                var tempPath = Path.Combine(destDirName, subDirectory.Name);
                DirectoryCopy(subDirectory.FullName, tempPath);
            }
        }
    }
}




