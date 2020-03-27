using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bake.Core;

namespace Bake.Tests.Helpers
{
    public abstract class ProjectTest
    {
        protected abstract string ProjectFolder { get; }

        protected int Execute(params string[] args)
        {
            var path = Path.Join(
                ProjectRoot.Get(),
                "TestProjects",
                ProjectFolder);
            var current = Directory.GetCurrentDirectory();

            args = args
                .Concat(
                    new[]
                    {
                        "--log-level", "debug"
                    })
                .ToArray();

            using (DisposableAction.With(() => Directory.SetCurrentDirectory(current)))
            {
                Directory.SetCurrentDirectory(path);
                return Program.Main(args);
            }
        }
    }
}
