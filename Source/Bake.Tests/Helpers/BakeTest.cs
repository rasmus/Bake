using System.Threading.Tasks;

namespace Bake.Tests.Helpers
{
    public abstract class BakeTest : TestProject
    {
        protected BakeTest(string projectName) : base(projectName)
        {
        }

        protected Task<int> ExecuteAsync(params string[] args)
        {
            return Program.Main(args);
        }
    }
}