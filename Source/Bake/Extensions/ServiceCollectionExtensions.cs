using Bake.Commands;
using Bake.Commands.Build;
using Bake.Commands.Init;
using Bake.Cooking;
using Bake.Cooking.Composers;
using Bake.Cooking.Cooks;
using Bake.Cooking.Cooks.DotNet;
using Bake.Services;
using Bake.ValueObjects.Recipes.DotNet;
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
                .AddTransient<IEditor, Editor>()
                .AddTransient<IKitchen, Kitchen>()
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<IRunnerFactory, RunnerFactory>()

                // Cli wrappers
                .AddTransient<IDotNet, DotNet>()

                // Composers
                .AddTransient<IComposer, DotNetComposer>()

                // Cooks
                .AddTransient<ICook, DotNetCleanCook>()
                .AddTransient<ICook, DotNetBuildCook>()
                .AddTransient<ICook, DotNetRestoreCook>()
                .AddTransient<ICook, DotNetTestCook>()

                // Commands
                .AddTransient<InitCommand>()
                .AddTransient<BuildCommand>();

            return serviceCollection;
        }
    }
}
