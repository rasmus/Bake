using Bake.Commands;
using Bake.Commands.Init;
using Bake.Services;
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
                .AddSingleton<IExecutor, Executor>()
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<IRunnerFactory, RunnerFactory>()

                // Commands
                .AddTransient<InitCommand>();

            return serviceCollection;
        }
    }
}
