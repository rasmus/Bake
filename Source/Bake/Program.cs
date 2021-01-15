using System.Linq;
using System.Threading.Tasks;
using Bake.Commands;
using Bake.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bake
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(b => b.AddConsole())
                .AddBake();

            var commandType = typeof(ICommand);
            var commandTypes = serviceCollection
                .Where(d => commandType.IsAssignableFrom(d.ImplementationType))
                .Select(d => d.ImplementationType)
                .ToList();

            int returnCode;
            using (var serviceProvider = serviceCollection.BuildServiceProvider(true))
            {
                var executor = serviceProvider.GetRequiredService<IExecutor>();
                returnCode = await executor.ExecuteAsync(args, commandTypes);
            }

            return returnCode;
        }
    }
}
