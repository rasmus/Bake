using System.Linq;
using System.Threading.Tasks;
using Bake.Commands;
using Bake.Core;
using Bake.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bake
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(f => f.AddSerilog(LoggerBuilder.CreateLogger()))
                .AddBake();

            var commandType = typeof(ICommand);
            var commandTypes = serviceCollection
                .Where(d => commandType.IsAssignableFrom(d.ImplementationType))
                .Select(d => d.ImplementationType)
                .ToList();

            await using var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var executor = serviceProvider.GetRequiredService<IExecutor>();
            var returnCode = await executor.ExecuteAsync(args, commandTypes);

            return returnCode;
        }
    }
}
