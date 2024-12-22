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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using Bake.Extensions;
using Bake.ValueObjects;
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

        public bool FileExists(string filePath)
        {
            return System.IO.File.Exists(filePath);
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
                if (ShouldBeSkipped(Path.GetDirectoryName(filePath)!))
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

        public async Task CopyFileAsync(
            string sourcePath,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            var destinationParentDirectory = Path.GetDirectoryName(destinationPath);
            if (string.IsNullOrEmpty(destinationParentDirectory))
            {
                throw new ArgumentException($"Cannot determine parent directory of {destinationPath}");
            }
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationParentDirectory!);
            }

            await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            await using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await sourceStream.CopyToAsync(destinationStream, 81920, cancellationToken);
        }

        public async Task CopyDirectoryAsync(
            string sourcePath,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            var directoryInfo = new DirectoryInfo(sourcePath);
            var directoryInfos = directoryInfo.GetDirectories();

            Directory.CreateDirectory(destinationPath);

            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destinationPath, file.Name);
                await CopyFileAsync(file.FullName, tempPath, cancellationToken);
            }

            foreach (var subDirectory in directoryInfos)
            {
                var tempPath = Path.Combine(destinationPath, subDirectory.Name);
                await CopyDirectoryAsync(subDirectory.FullName, tempPath, cancellationToken);
            }
        }

        public async Task<IFile> CompressAsync(
            string fileName,
            CompressionAlgorithm algorithm,
            IReadOnlyCollection<IFile> files,
            CancellationToken cancellationToken)
        {
            if (algorithm != CompressionAlgorithm.ZIP)
            {
                throw new ArgumentOutOfRangeException(nameof(algorithm));
            }

            if (files.Count == 0)
            {
                throw new ArgumentNullException(nameof(files));
            }

            var totalSize = files.Sum(f => f.Size);
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "Compressing file {FileNames} of total size {TotalSize} into {FileName}",
                files.Select(f => f.FileName).ToArray(),
                totalSize.BytesToString(),
                fileName);

            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                $"bake-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directoryPath);
            var filePath = Path.Combine(directoryPath, fileName);

            await using var zipFileStream = System.IO.File.Open(filePath, FileMode.CreateNew);
            using var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);

            foreach (var file in files)
            {
                var zipArchiveEntry = zipArchive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                await using var zipArchiveEntrySteam = zipArchiveEntry.Open();
                await using var fileStream = await file.OpenReadAsync(cancellationToken);
                await fileStream.CopyToAsync(zipArchiveEntrySteam, 4096, cancellationToken);
            }

            var zipFile = new File(filePath);
            _logger.LogInformation(
                "Finishing creating file {FileName} after {TotalSeconds} seconds with a size of {Size} (compression {CompressionRatio})",
                fileName,
                stopwatch.Elapsed.TotalSeconds,
                zipFile.Size.BytesToString(),
                $"{(zipFile.Size * 100.0)/totalSize:0.#}%");

            return zipFile;
        }

        public async Task<string> ReadAllTextAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            await using var file = System.IO.File.Open(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            using var streamReader = new StreamReader(filePath);

            return await streamReader.ReadToEndAsync();
        }

        public IFile Get(string filePath)
        {
            return new File(filePath);
        }

        public IFile OpenTempFile()
        {
            return new File(
                Path.GetTempFileName());
        }

        private bool ShouldBeSkipped(string directoryPath)
        {
            var originalDirectoryPath = directoryPath;

            while (true)
            {
                if (ShouldSkipCache.TryGetValue(directoryPath, out var shouldBeSkipped))
                {
                    if (shouldBeSkipped)
                    {
                        _logger.LogDebug(
                            "Skipping {DirectoryPath} we have previously determined that it should be skipped",
                            originalDirectoryPath);
                    }

                    return shouldBeSkipped;
                }

                if (string.Equals("node_modules", Path.GetFileName(directoryPath)))
                {
                    _logger.LogInformation(
                        "Skipping {DirectoryPath} as {NodeModulesDirectoryPath} is a 'node_modules' directory",
                        originalDirectoryPath,
                        directoryPath);
                    ShouldSkipCache[directoryPath] = true;
                    return true;
                }

                var hasSkipFile = Directory
                    .GetFiles(directoryPath)
                    .Any(f => string.Equals(".bake-ignore", Path.GetFileName(f), StringComparison.OrdinalIgnoreCase));

                if (hasSkipFile)
                {
                    _logger.LogInformation(
                        "Skipping {DirectoryPath} as {CurrentDirectoryPath} contains a '.bake-ignore' file",
                        originalDirectoryPath,
                        directoryPath);
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
