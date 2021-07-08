// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
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

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Core
{
    public class FileSystemTests : TestFor<FileSystem>
    {
        [Test]
        public async Task FindFilesAsync()
        {
            // Arrange
            await using var folder = await CreateFolderStructureAsync(
                "file.txt",
                "folder-1/folder-1-1/file-1-1.txt",
                "folder-1/file-1.txt",
                "folder-2/.bake-ignore",
                "folder-2/file-2.txt",
                "folder-2/folder-2-2/file-2-2.txt");

            // Act
            var files = await Sut.FindFilesAsync(
                folder.Path,
                "*.txt",
                CancellationToken.None);

            // Assert
            files.Should().HaveCount(3);
        }

        private static async Task<Folder> CreateFolderStructureAsync(
            params string[] files)
        {
            var folder = Folder.New;
            try
            {
                foreach (var file in files)
                {
                    var filePath = Path.Combine(folder.Path, Path.Combine(file.Split('/')));
                    var directoryPath = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    await System.IO.File.WriteAllTextAsync(filePath, string.Empty);
                }
            }
            finally
            {
                await folder.DisposeAsync();
            }

            return folder;
        }
    }
}
