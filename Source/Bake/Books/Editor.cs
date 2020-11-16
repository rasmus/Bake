using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Books
{
    public class Editor : IEditor
    {
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            IEnumerable<IComposer> composers)
        {
            _composers = composers.ToList();
        }

        public Task ComposeAsync(
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
