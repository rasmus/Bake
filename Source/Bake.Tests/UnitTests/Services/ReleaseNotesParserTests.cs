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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using FluentAssertions;
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
            releaseNotes.SingleOrDefault(n => n.Version == SemVer.Parse("0.9")).Should().NotBeNull();
            releaseNotes.SingleOrDefault(n => n.Version == SemVer.Parse("1.0-alpha")).Should().NotBeNull();
        }
    }
}
