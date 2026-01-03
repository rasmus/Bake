// MIT License
// 
// Copyright (c) 2021-2025 Rasmus Mikkelsen
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

using Bake.Cooking.Cooks.Release;
using Bake.Core;
using Bake.Tests.Helpers;
using Bake.ValueObjects.Recipes.Release;
using Bake.ValueObjects.Releases;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using File = System.IO.File;

namespace Bake.Tests.UnitTests.Cooking.Cooks
{
    public class ReleaseCookTests : TestService<ReleaseCook>
    {
        [Test]
        public async Task Empty()
        {
            // Arrange
            var context = NewContext();

            // Act
            var success = await Sut.CookAsync(
                context,
                new ReleaseRecipe(
                    string.Empty,
                    []),
                Timeout);

            // Assert
            success.Should().BeTrue();
        }

        [Test]
        public async Task Files()
        {
            // Arrange
            var context = NewContext();

            // Act
            var success = await Sut.CookAsync(
                context,
                new ReleaseRecipe(
                    string.Empty,
                    [
                        NewReleaseFile()
                    ]),
                Timeout);

            // Assert
            success.Should().BeTrue();
        }

        [Test]
        public async Task Directories()
        {
            // Arrange
            var context = NewContext();

            // Act
            var success = await Sut.CookAsync(
                context,
                new ReleaseRecipe(
                    string.Empty,
                    [
                        NewReleaseDirectory()
                    ]),
                Timeout);

            // Assert
            success.Should().BeTrue();
        }

        [Test]
        public async Task Mixed()
        {
            // Arrange
            var context = NewContext();

            // Act
            var success = await Sut.CookAsync(
                context,
                new ReleaseRecipe(
                    string.Empty,
                    [
                        NewMixedRelease(),
                        NewReleaseDirectory(),
                        NewReleaseFile(),
                    ]),
                Timeout);

            // Assert
            success.Should().BeTrue();
        }

        private static Context NewContext()
        {
            return Context.New(ValueObjects.Ingredients.New(SemVer.Random, Path.GetTempPath()));
        }

        private ReleaseFile NewMixedRelease()
        {
            var fileName = $"{Guid.NewGuid():N}.zip";
            var destinationPath = Path.Combine(Path.GetTempPath(), fileName);
            DeleteAfter(destinationPath);

            return new ReleaseFile(
                fileName,
                [NewDirectory(), NewFile(), NewFile(), NewDirectory()],
                destinationPath);
        }

        private ReleaseFile NewReleaseDirectory()
        {
            var fileName = $"{Guid.NewGuid():N}.zip";
            var destinationPath = Path.Combine(Path.GetTempPath(), fileName);
            DeleteAfter(destinationPath);

            return new ReleaseFile(
                fileName,
                [NewDirectory()],
                destinationPath);
        }

        private ReleaseFile NewReleaseFile(int fileCount = 3)
        {
            var fileName = $"{Guid.NewGuid():N}.zip";
            var destinationPath = Path.Combine(Path.GetTempPath(), fileName);
            DeleteAfter(destinationPath);

            return new ReleaseFile(
                fileName,
                Enumerable.Range(0, fileCount).Select(_ => NewFile()).ToArray(),
                destinationPath);
        }

        private string NewDirectory(params string[] path)
        {
            var name = Guid.NewGuid().ToString("N");
            path = path.Concat([name]).ToArray();
            var directory = path.Aggregate(Path.GetTempPath(), Path.Combine);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _ = Enumerable.Range(0, 3).Select(_ => NewFile(directory)).ToArray();

            return directory;
        }

        private string NewFile(params string[] path)
        {
            var parentDirectory = path.Aggregate(Path.GetTempPath(), Path.Combine);
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            var filePath = Path.Combine(
                parentDirectory,
                $"{Guid.NewGuid():N}.txt");

            File.WriteAllText(filePath, "Hello there!");

            DeleteAfter(filePath);

            return filePath;
        }

        protected override IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return base.Configure(serviceCollection)
                .AddTransient<IFileSystem, FileSystem>();
        }
    }
}
