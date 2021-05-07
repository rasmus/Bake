using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects;

namespace Bake.Services
{
    public interface IReleaseNotesParser
    {
        Task<ReleaseNotes> ParseAsync(
            string path,
            CancellationToken cancellationToken);
    }
}
