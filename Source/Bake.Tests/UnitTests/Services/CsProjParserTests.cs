using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class CsProjParserTests : TestFor<CsProjParser>
    {
        [Test]
        public async Task IsTool()
        {
            // Arrange
            var path = await WriteEmbeddedAsync("dotnet-tool-csproj.xml");
            
            // Act
            var csProj = await Sut.ParseAsync(path, CancellationToken.None);

            // Assert
            csProj.PackAsTool.Should().BeTrue();
            csProj.IsPackable.Should().BeFalse();
            csProj.IsPublishable.Should().BeFalse();
        }
    }
}
