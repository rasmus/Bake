using Bake.Books;
using Bake.Books.Composers;
using Bake.Books.Cooks;
using Bake.Books.Cooks.DotNet;
using Bake.Books.Recipes.DotNet;
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

                // Composers
                .AddTransient<IComposer, DotNetComposer>()

                // Cooks
                .AddTransient<ICook, DotNetCleanCook>()
                .AddTransient<ICook, DotNetBuildCook>()

                // Commands
                .AddTransient<InitCommand>();

            return serviceCollection;
        }
    }
}
