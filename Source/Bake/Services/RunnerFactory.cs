using System.IO;
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class RunnerFactory : IRunnerFactory
    {
        private readonly ILogger<RunnerFactory> _logger;

        public RunnerFactory(
            ILogger<RunnerFactory> logger)
        {
            _logger = logger;
        }

        public IRunner CreateRunner(
            string command,
            string workingDirectory,
            params string[] arguments)
        {
            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            return new Runner(
                _logger,
                command,
                workingDirectory,
                arguments);
        }
    }
}
