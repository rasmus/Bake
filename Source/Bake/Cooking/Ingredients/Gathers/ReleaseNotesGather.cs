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
        private readonly IRandom _random;

        public ReleaseNotesGather(
            ILogger<ReleaseNotesGather> logger,
            IReleaseNotesParser releaseNotesParser,
            IRandom random)
        {
            _logger = logger;
            _releaseNotesParser = releaseNotesParser;
            _random = random;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            var releaseNotesPath = Path.Combine(
                ingredients.WorkingDirectory,
                "RELEASE_NOTES.md");

            if (!System.IO.File.Exists(releaseNotesPath))
            {
                _logger.LogInformation(
                    "No release notes found at {ReleaseNotesPath}",
                    releaseNotesPath);
                return;
            }

            var releaseNotes = await _releaseNotesParser.ParseAsync(
                releaseNotesPath,
                cancellationToken);

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

                if (withExactVersion.Count > 1)
                {
                    _logger.LogWarning(
                        "Found {Count} release notes with version {Version}, just picking one at random",
                        withExactVersion.Count,
                        version.ToString());

                    // Instead of having system rely on the ordering of some implementation
                    // we might as well pick one randomly... or should we fail the build?
                    return withExactVersion[_random.NextInt(withExactVersion.Count)];
                }
            }
            else
            {
                var withSubset = releaseNotes
                    .Where(n => version.IsSubset(n.Version))
                    .OrderByDescending(n => n.Version)
                    .ToList();

                if (withSubset.Any())
                {
                    _logger.LogInformation(
                        "Found {Count} release notes that matches {Version}, picking the most recent",
                        withSubset.Count,
                        version.ToString());

                    return withSubset.First();
                }
            }

            var orderedReleaseNotes = releaseNotes
                .OrderByDescending(n => n.Version)
                .ToList();

            _logger.LogWarning(
                "Found {Count} releases, none matches the version {Version}, picking the most recent",
                orderedReleaseNotes.Count,
                version.ToString());

            return orderedReleaseNotes.First();
        }
    }
}
