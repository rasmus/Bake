using System.Linq;
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

        [TestCase(
            "1.0.0 1.1.0 1.1.1",
            "1.1.1",
            "1.0.0",
            "1.1.0")]
        [TestCase(
            "0.0.1 0.1.0 1.0.0",
            "1.0.0",
            "0.0.1",
            "0.1.0")]
        public void Compare(
            string expectedSorting,
            params string[] versions)
        {
            // Act
            var list = versions
                .Select(SemVer.Parse)
                .OrderBy(v => v);

            // Assert
            string.Join(" ", list.Select(v => v.ToString())).Should().Be(expectedSorting);
        }
    }
}
