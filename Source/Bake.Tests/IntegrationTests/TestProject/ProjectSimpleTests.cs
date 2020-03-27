using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.TestProject
{
    public class ProjectSimpleTests : ProjectTest
    {
        protected override string ProjectFolder => "Simple";

        [Test]
        public void Build()
        {
            // Act
            var exitCode = Execute();

            // Assert
            exitCode.Should().Be(0);
        }
    }
}
