using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Tests.Helpers;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class ReleaseNotesParserTests : TestFor<ReleaseNotesParser>
    {
        [Test]
        public async Task CanParse()
        {
            // Arrange
            var path = await WriteEmbeddedAsync("release-notes.md");

            // Act
            var releaseNotes = await Sut.ParseAsync(
                path,
                CancellationToken.None);

            // Assert
        }
    }
}
