using System.Threading;
using System.Threading.Tasks;

namespace Bake.Commands.Init
{
    [CommandVerb("run")]
    public class RunCommand : ICommand
    {
        public Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
