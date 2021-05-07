using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class ReleaseNotesGather : IGather
    {
        private readonly ILogger<ReleaseNotesGather> _logger;
        private readonly IReleaseNotesParser _releaseNotesParser;

        public ReleaseNotesGather(
            ILogger<ReleaseNotesGather> logger,
            IReleaseNotesParser releaseNotesParser)
        {
            _logger = logger;
            _releaseNotesParser = releaseNotesParser;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            var releaseNotesPath = Path.Combine(
                ingredients.WorkingDirectory,
                "RELEASE_NOTES.md");

            if (!File.Exists(releaseNotesPath))
            {
                _logger.LogInformation(
                    "No release notes found at {ReleaseNotesPath}",
                    releaseNotesPath);
                return;
            }

            var releaseNotes = await _releaseNotesParser.ParseAsync(
                releaseNotesPath,
                cancellationToken);

            ingredients.ReleaseNotes = releaseNotes;
        }
    }
}
