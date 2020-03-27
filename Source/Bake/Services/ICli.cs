namespace Bake.Services
{
    public interface ICli
    {
        ICommand CreateCommand(
            string command,
            string workingDirectory,
            params string[] arguments);
    }
}
