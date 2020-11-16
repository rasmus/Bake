using System.Collections.Generic;
using System.Linq;
using Bake.Services;

namespace Bake.Extensions
{
    public static class RunnerFactoryExtensions
    {
        public static IRunner CreateRunner(
            this IRunnerFactory runnerFactory,
            string command,
            string workingDirectory,
            IEnumerable<string> arguments)
        {
            return runnerFactory.CreateRunner(
                command,
                workingDirectory,
                arguments.ToArray());
        }
    }
}
