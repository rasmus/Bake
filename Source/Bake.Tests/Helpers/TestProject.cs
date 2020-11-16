using System.IO;
using System.Threading.Tasks;
using Bake.Core;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public abstract class TestProject
    {
        protected string ProjectName { get; }
        protected string WorkingDirectory => _folder.Path;

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

            DirectoryCopy(
                Path.Combine(
                    ProjectHelper.GetRoot(),
                    "TestProjects",
                    ProjectName),
                _folder.Path);
        }

        [TearDown]
        public async Task TearDownTestProject()
        {
            await _folder.DisposeAsync();
        }

        protected Task<int> ExecuteAsync(params string[] args)
        {
            return Program.Main(args);
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
