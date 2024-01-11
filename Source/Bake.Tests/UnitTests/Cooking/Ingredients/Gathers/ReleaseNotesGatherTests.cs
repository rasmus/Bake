// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using Bake.Cooking.Ingredients.Gathers;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Cooking.Ingredients.Gathers
{
    public class ReleaseNotesGatherTests : TestFor<ReleaseNotesGather>
    {
        private IReleaseNotesParser _releaseNotesParserMock = null!;
        private IFileSystem _fileSystemMock = null!;

        [SetUp]
        public void SetUp()
        {
            _releaseNotesParserMock = InjectMock<IReleaseNotesParser>();
            _fileSystemMock = InjectMock<IFileSystem>();
        }

        [TestCase(
            "1.0.129",
            "1.1,1.0,0.9",
            "1.0")]
        [TestCase(
            "1.0",
            "1.1,1.0,0.9",
            "1.0")]
        [TestCase(
            "1.0.129",
            "1.1,1.0-alpha,0.9",
            "1.0-alpha")]
        public async Task PickExpectedVersion(
            string version,
            string releaseNoteVersions,
            string expectedVersion)
        {
            // Arrange
            var releaseNotes = (IReadOnlyCollection<ReleaseNotes>) releaseNoteVersions
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => new ReleaseNotes(SemVer.Parse(v), A<string>()))
                .OrderByDescending(n => n.Version)
                .ToArray();
            var ingredients = ValueObjects.Ingredients.New(
                SemVer.Parse(version),
                A<string>());
            _fileSystemMock
                .FileExists(Arg.Any<string>())
                .Returns(true);
            _releaseNotesParserMock
                .ParseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(releaseNotes));

            // Act
            await Sut.GatherAsync(ingredients, CancellationToken.None);

            // Assert
            ingredients.ReleaseNotes!.Version.ToString().Should().Be(expectedVersion);
        }
    }
}
