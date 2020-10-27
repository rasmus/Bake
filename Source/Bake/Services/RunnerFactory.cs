using System.IO;

namespace Bake.Services
{
    public class RunnerFactory : IRunnerFactory
    {
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
                command,
                workingDirectory,
                arguments);
        }
    }
}
