using System.IO;

namespace Bake.Services
{
    public class Cli : ICli
    {
        public ICommand CreateCommand(
            string command,
            string workingDirectory,
            params string[] arguments)
        {
            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            return new Command(
                command,
                workingDirectory,
                arguments);
        }
    }
}
