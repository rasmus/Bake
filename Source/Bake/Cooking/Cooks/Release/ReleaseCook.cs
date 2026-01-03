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

using Bake.Core;
using Bake.ValueObjects.Recipes.Release;
using Bake.ValueObjects.Releases;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using Bake.Extensions;
using File = System.IO.File;

namespace Bake.Cooking.Cooks.Release
{
    public class ReleaseCook : Cook<ReleaseRecipe>
    {
        private readonly ILogger<ReleaseCook> _logger;
        private readonly IFileSystem _fileSystem;

        public ReleaseCook(
            ILogger<ReleaseCook> logger,
            IFileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            ReleaseRecipe recipe,
            CancellationToken cancellationToken)
        {
            foreach (var releaseFile in recipe.Files)
            {
                if (!await CompressReleaseFilesAsync(releaseFile, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CompressReleaseFilesAsync(ReleaseFile releaseFile, CancellationToken cancellationToken)
        {
            var tmpDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _logger.LogInformation("Creating temporary directory at {TmpDirectory}", tmpDirectory);
            foreach (var source in releaseFile.Sources)
            {
                if (File.Exists(source))
                {
                    var fileName = Path.GetFileName(source);
                    var destination = Path.Combine(tmpDirectory, fileName);
                    _logger.LogInformation("Copying file from {Source} to {Destination}", source, destination);
                    await _fileSystem.CopyFileAsync(source, destination, cancellationToken);
                }
                else if (Directory.Exists(source))
                {
                    _logger.LogInformation("Copying directory from {Source} to {Destination}", source, tmpDirectory);
                    await _fileSystem.CopyDirectoryAsync(source, tmpDirectory, cancellationToken);
                }
                else
                {
                    _logger.LogError("The source {Source} does not exist", source);
                    return false;
                }
            }

            var destinationDirectory = Path.GetDirectoryName(releaseFile.Destination);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            _logger.LogInformation("Creating ZIP file at {Destination}", releaseFile.Destination);
            ZipFile.CreateFromDirectory(tmpDirectory, releaseFile.Destination);
            var fileInfo = new FileInfo(releaseFile.Destination);
            _logger.LogInformation("Created ZIP file {Destination} with size {Size}", releaseFile.Destination, fileInfo.Length.BytesToString());

            return true;
        }
    }
}
