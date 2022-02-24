// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
            null,
            null)]
        [TestCase(
            "1.2.3-meta",
            1,
            2,
            3,
            "meta")]
        [TestCase(
            "321.231.132-magic",
            321,
            231,
            132,
            "magic")]
        public void ValidVersions(
            string str,
            int expectedMajor,
            int expectedMinor,
            int? expectedPatch,
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

        [TestCase("1.2"  , "1.2.0", true)]
        [TestCase("1.2"  , "1.2.5", true)]
        [TestCase("1.2.0", "1.2.0", true)]
        [TestCase("1.2.1", "1.2.0", false)]
        [TestCase("2.1.1", "1.2.1", false)]
        public void IsSubset(
            string versionStr,
            string otherStr,
            bool expectedResult)
        {
            // Arrange
            var version = SemVer.Parse(versionStr);
            var other = SemVer.Parse(otherStr);

            // Act
            var result = version.IsSubset(other);

            // Assert
            result.Should().Be(expectedResult);
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
        [TestCase(
            "1.0 1.0-alpha",
            "1.0",
            "1.0-alpha")]
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
