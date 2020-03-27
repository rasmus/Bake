using System.IO;
using System.Threading.Tasks;
using Bake.Core;

namespace Bake.Tests.Helpers
{
    public abstract class ProjectTest
    {
        protected abstract string ProjectFolder { get; }

        protected async Task<int> ExecuteAsync(params string[] args)
        {
            var path = Path.Join(
                ProjectRoot.Get(),
                "TestProjects",
                ProjectFolder);
            var current = Directory.GetCurrentDirectory();

            using (DisposableAction.With(() => Directory.SetCurrentDirectory(current)))
            {
                Directory.SetCurrentDirectory(path);
                return await Program.Main(args);
            }
        }
    }
}
