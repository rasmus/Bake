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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class ReleaseNotesParser : IReleaseNotesParser
    {
        private readonly ILogger<ReleaseNotesParser> _logger;

        public ReleaseNotesParser(
            ILogger<ReleaseNotesParser> logger)
        {
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<ReleaseNotes>> ParseAsync(
            string path,
            CancellationToken cancellationToken)
        {
            var markdown = await System.IO.File.ReadAllTextAsync(
                path,
                cancellationToken);

            var lines = markdown.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

            return ParseLines(lines)
                .OrderByDescending(n => n.Version)
                .ToArray();
        }

        private IEnumerable<ReleaseNotes> ParseLines(IEnumerable<string> lines)
        {
            List<string>? releaseNotes = null;
            SemVer? version = null;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("#"))
                {
                    if (releaseNotes != null)
                    {
                        yield return Create(version!, releaseNotes);
                        releaseNotes = null;
                    }

                    version = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => SemVer.TryParse(w, out var v) ? v : null)
                        .SingleOrDefault(w => w != null);

                    if (version != null)
                    {
                        releaseNotes = new List<string>();
                    }
                    else
                    {
                        _logger.LogWarning("Skipping section, cannot understand header {Header}", line);
                    }
                }
                else
                {
                    releaseNotes?.Add(line.TrimEnd());
                }
            }

            if (releaseNotes != null)
            {
                yield return Create(version!, releaseNotes);
            }
        }

        private ReleaseNotes Create(SemVer version, IReadOnlyCollection<string> lines)
        {
            var notes = string.Join(Environment.NewLine, lines);

            _logger.LogTrace(
                "Creating release notes for version {Version} with {LineCount} lines",
                version,
                lines.Count);

            return new ReleaseNotes(
                version,
                notes);
        }
    }
}
