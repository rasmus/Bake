using Bake.Commands;
using Bake.Commands.Init;
using Microsoft.Extensions.DependencyInjection;

namespace Bake.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBake(
            this IServiceCollection serviceCollection)
        {
            serviceCollection
                // Core
                .AddTransient<IExecutor, Executor>()
                .AddTransient<ICommandFactory, CommandFactory>()

                // Commands
                .AddTransient<InitCommand>();

            return serviceCollection;
        }
    }
}
