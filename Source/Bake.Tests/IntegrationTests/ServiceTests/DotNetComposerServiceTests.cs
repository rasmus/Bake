using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Composers;
using Bake.Core;
using Bake.Tests.Helpers;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.ServiceTests
{
    public class DotNetComposerServiceTests : ServiceTest<DotNetComposer>
    {
        public DotNetComposerServiceTests() : base("NetCore.Console")
        {
        }

        [Test]
        public async Task T()
        {
            // Act
            var recipes = await Sut.ComposeAsync(
                new Context(SemVer.Random), 
                WorkingDirectory,
                CancellationToken.None);
        }
    }
}
