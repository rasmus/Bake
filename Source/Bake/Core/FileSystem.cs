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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bake.Core
{
    public class FileSystem : IFileSystem
    {
        private static readonly ConcurrentDictionary<string, bool> ShouldSkipCache = new();

        private readonly ILogger<FileSystem> _logger;

        public FileSystem(
            ILogger<FileSystem> logger)
        {
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<string>> FindFilesAsync(
            string directoryPath,
            string searchPattern,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug(
                "Scanning {Directory} for pattern {SearchPattern}",
                directoryPath,
                searchPattern);

            var filePaths = await Task.Factory.StartNew(
                () => (IReadOnlyCollection<string>) Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            var validPaths = new List<string>();
            var skippedPaths = new List<string>();

            foreach (var filePath in filePaths)
            {
                if (ShouldBeSkipped(Path.GetDirectoryName(filePath)))
                {
                    skippedPaths.Add(filePath);
                }
                else
                {
                    validPaths.Add(filePath);
                }
            }

            _logger.LogDebug(
                "Found these files {ValidFiles}, but skipped these {SkippedFiles}",
                validPaths,
                skippedPaths);

            return validPaths;
        }

        public async Task<string> ReadAllTextAsync(
            string filePath,
            CancellationToken _)
        {
            await using var file = System.IO.File.Open(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            using var streamReader = new StreamReader(filePath);

            return await streamReader.ReadToEndAsync();
        }

        public IFile Open(string filePath)
        {
            return new File(filePath);
        }

        public IFile OpenTempFile()
        {
            return new File(
                Path.GetTempFileName());
        }

        private static bool ShouldBeSkipped(string directoryPath)
        {
            while (true)
            {
                if (ShouldSkipCache.TryGetValue(directoryPath, out var shouldBeSkipped))
                {
                    return shouldBeSkipped;
                }

                var hasSkipFile = Directory
                    .GetFiles(directoryPath)
                    .Any(f => string.Equals(".bake-ignore", Path.GetFileName(f), StringComparison.OrdinalIgnoreCase));

                if (hasSkipFile)
                {
                    ShouldSkipCache[directoryPath] = true;
                    return true;
                }

                var parentDirectory = new DirectoryInfo(directoryPath).Parent?.FullName;
                if (string.IsNullOrEmpty(parentDirectory))
                {
                    ShouldSkipCache[directoryPath] = false;
                    return false;
                }

                directoryPath = parentDirectory;
            }
        }
    }
}
