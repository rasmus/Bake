using Bake.Core;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Core
{
    public class SemVerTests
    {
        [TestCase(
            "1.2",
            1,
            2,
            0,
            null)]
        [TestCase(
            "1.2.3-meta",
            1,
            2,
            3,
            "meta")]
        public void ValidVersions(
            string str,
            int expectedMajor,
            int expectedMinor,
            int expectedPatch,
            string expectedMeta)
        {
            // Act
            var version = SemVer.Parse(str);

            // Assert
            version.Major.Should().Be(expectedMajor);
            version.Minor.Should().Be(expectedMinor);
            version.Patch.Should().Be(expectedPatch);
            version.Meta.Should().Be(expectedMeta ?? string.Empty);
        }
    }
}
