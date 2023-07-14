// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class ReleaseNotesGather : IGather
    {
        private readonly ILogger<ReleaseNotesGather> _logger;
        private readonly IReleaseNotesParser _releaseNotesParser;
        private readonly IFileSystem _fileSystem;

        public ReleaseNotesGather(
            ILogger<ReleaseNotesGather> logger,
            IReleaseNotesParser releaseNotesParser,
            IFileSystem fileSystem)
        {
            _logger = logger;
            _releaseNotesParser = releaseNotesParser;
            _fileSystem = fileSystem;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            var releaseNotesPath = Path.Combine(
                ingredients.WorkingDirectory,
                "RELEASE_NOTES.md");

            if (!_fileSystem.FileExists(releaseNotesPath))
            {
                _logger.LogInformation(
                    "No release notes found at {ReleaseNotesPath}",
                    releaseNotesPath);
                ingredients.FailReleaseNotes();
                return;
            }

            var releaseNotes = await _releaseNotesParser.ParseAsync(
                releaseNotesPath,
                cancellationToken);

            if (!releaseNotes.Any())
            {
                _logger.LogInformation(
                    "Was not able to parse any release notes from {ReleaseNotesPath}",
                    releaseNotesPath);
                ingredients.FailReleaseNotes();
                return;
            }

            _logger.LogInformation(() =>
            {
                var versions = releaseNotes
                    .Select(n => n.Version)
                    .OrderByDescending(v => v)
                    .Select(v => v.ToString())
                    .ToList();

                return (
                    "Found {ReleaseNoteCount} release notes with versions {ReleaseNoteVersions}",
                    new object[] {versions.Count, versions}
                    );
            });

            ingredients.ReleaseNotes = PickReleaseNotes( 
                ingredients.Version,
                releaseNotes);
        }

        private ReleaseNotes PickReleaseNotes(
            SemVer version,
            IReadOnlyCollection<ReleaseNotes> releaseNotes)
        {
            if (!releaseNotes.Any())
            {
                return null;
            }

            if (version.Patch.HasValue)
            {
                var withExactVersion = releaseNotes
                    .Where(n => version.Equals(n.Version))
                    .ToList();

                if (withExactVersion.Count == 1)
                {
                    return withExactVersion.Single();
                }
            }

            var withSubset = releaseNotes
                .Where(n => n.Version.IsSubset(version))
                .OrderByDescending(n => n.Version)
                .ToList();

            if (withSubset.Any())
            {
                var notes = withSubset.First();

                _logger.LogInformation(
                    "Found {Count} release notes that matches {Version}, picking the most recent. Got {PickedVersion}",
                    withSubset.Count,
                    version.ToString(),
                    notes.Version.ToString());

                return notes;
            }

            var orderedReleaseNotes = releaseNotes
                .OrderByDescending(n => n.Version)
                .ToList();
            var pickedNotes = orderedReleaseNotes.First();

            _logger.LogWarning(
                "Found {Count} releases, none matches the version {Version}, picking the most recent. Got {PickedVersion}",
                orderedReleaseNotes.Count,
                version.ToString(),
                pickedNotes.Version.ToString());

            return pickedNotes;
        }
    }
}
