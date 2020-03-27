using System.Threading.Tasks;
using Bake.Tests.Helpers;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.TestProject
{
    public class ProjectSimpleTests : ProjectTest
    {
        protected override string ProjectFolder => "Simple";

        [Test]
        public async Task Build()
        {
            await ExecuteAsync();
        }
    }
}
