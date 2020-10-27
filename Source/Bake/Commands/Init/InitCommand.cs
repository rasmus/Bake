using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Commands.Init
{
    [CommandVerb("init")]
    public class InitCommand : ICommand
    {
        public Task<int> ExecuteAsync(
            [Argument("the magic")] int magicNumber,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("init");

            return Task.FromResult(0);
        }
    }
}
