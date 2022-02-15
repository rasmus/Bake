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

using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Tests.Helpers;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.DotNet;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class YamlTests : TestFor<Yaml>
    {
        [TestCase("1.2.3")]
        public async Task SerializeSemVer(string str)
        {
            // Arrange
            var semVer = SemVer.Parse(str);

            // Act
            var yaml = await Sut.SerializeAsync(semVer, CancellationToken.None);

            // Assert
            yaml.Trim().Should().Be($"\"{str}\"");
        }

        [TestCase("1.2.3")]
        public async Task DeserializeSemVer(string str)
        {
            // Act
            var semVer = await Sut.DeserializeAsync<SemVer>(str, CancellationToken.None);

            // Assert
            semVer.ToString().Should().Be(str);
        }

        [Test]
        public async Task Recipes()
        {
            // Arrange
            var yaml = Lines(
                "- !dotnet-restore",
                "  path: /tmp",
                "  clearLocalHttpCache: true");

            // Act
            var recipes = await Sut.DeserializeAsync<Recipe[]>(yaml, CancellationToken.None);
        }

        [Test]
        public async Task Recipe()
        {
            // Arrange
            var yaml = Lines(
                "!dotnet-restore",
                "path: /tmp",
                "clearLocalHttpCache: true");

            // Act
            var recipe = await Sut.DeserializeAsync<Recipe>(yaml, CancellationToken.None);

            // Assert
            var dotNetRestore = recipe as DotNetRestoreSolutionRecipe;
            dotNetRestore.Should().NotBeNull();
            dotNetRestore.Path.Should().Be("/tmp");
            dotNetRestore.ClearLocalHttpCache.Should().BeTrue();
        }
    }
}
