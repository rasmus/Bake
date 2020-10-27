namespace Bake.Services
{
    public interface IRunnerFactory
    {
        IRunner CreateRunner(
            string command,
            string workingDirectory,
            params string[] arguments);
    }
}
