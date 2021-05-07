using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<ReleaseNotes> ParseAsync(
            string path,
            CancellationToken cancellationToken)
        {
            var markdown = await File.ReadAllTextAsync(
                path,
                cancellationToken);

            var lines = markdown.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

            var releaseNotes = ParseLines(lines).ToList();

            return releaseNotes.FirstOrDefault();
        }

        private IEnumerable<ReleaseNotes> ParseLines(IEnumerable<string> lines)
        {
            List<string> releaseNotes = null;
            SemVer version = null;

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
                        yield return new ReleaseNotes(version, string.Join(Environment.NewLine, releaseNotes));
                        version = null;
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
                yield return new ReleaseNotes(version, string.Join(Environment.NewLine, releaseNotes));
            }
        }
    }
}
