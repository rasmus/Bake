using System.Threading;
using System.Threading.Tasks;

namespace Bake.Services
{
    public class DotNet : IDotNet
    {
        private readonly IRunnerFactory _runnerFactory;

        public DotNet(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public async Task BuildAsync(
            string workingDirectory,
            CancellationToken cancellationToken)
        {
            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                workingDirectory,
                "build");

            var result = await buildRunner.ExecuteAsync(cancellationToken);
        }
    }
}
