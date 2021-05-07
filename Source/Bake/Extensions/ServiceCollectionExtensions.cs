using Bake.Commands;
using Bake.Commands.Init;
using Bake.Commands.Run;
using Bake.Cooking;
using Bake.Cooking.Composers;
using Bake.Cooking.Cooks;
using Bake.Cooking.Cooks.DotNet;
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
                .AddTransient<IEditor, Editor>()
                .AddTransient<IKitchen, Kitchen>()
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<IRunnerFactory, RunnerFactory>()
                .AddTransient<ICsProjParser, CsProjParser>()

                // Cli wrappers
                .AddTransient<IDotNet, DotNet>()

                // Composers
                .AddTransient<IComposer, DotNetComposer>()

                // Cooks
                .AddTransient<ICook, DotNetCleanCook>()
                .AddTransient<ICook, DotNetBuildCook>()
                .AddTransient<ICook, DotNetRestoreCook>()
                .AddTransient<ICook, DotNetTestCook>()
                .AddTransient<ICook, DotNetPackCook>()
                .AddTransient<ICook, DotNetNuGetPushCook>()

                // Commands
                .AddTransient<InitCommand>()
                .AddTransient<RunCommand>();

            return serviceCollection;
        }
    }
}
