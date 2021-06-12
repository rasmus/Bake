// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Bake.Commands;
using Bake.Commands.Apply;
using Bake.Commands.Plan;
using Bake.Commands.Run;
using Bake.Cooking;
using Bake.Cooking.Composers;
using Bake.Cooking.Cooks;
using Bake.Cooking.Cooks.Docker;
using Bake.Cooking.Cooks.DotNet;
using Bake.Cooking.Ingredients.Gathers;
using Bake.Core;
using Bake.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bake.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBake(
            this IServiceCollection serviceCollection,
            ILogCollector logCollector)
        {
            serviceCollection
                // Core
                .AddSingleton<IExecutor, Executor>()
                .AddTransient<IEditor, Editor>()
                .AddTransient<IKitchen, Kitchen>()
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<IRunnerFactory, RunnerFactory>()
                .AddTransient<ICsProjParser, CsProjParser>()
                .AddTransient<IReleaseNotesParser, ReleaseNotesParser>()
                .AddSingleton<IYaml, Yaml>()
                .AddTransient<IDocker, Docker>()
                .AddTransient<IConventionInterpreter, ConventionInterpreter>()
                .AddSingleton(logCollector)
                .AddSingleton<IRandom, RandomWrapper>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<ICredentials, Credentials>()

                // Gathers
                .AddTransient<IGather, GitGather>()
                .AddTransient<IGather, ReleaseNotesGather>()

                // Cli wrappers
                .AddTransient<IDotNet, DotNet>()

                // Composers
                .AddTransient<IComposer, DotNetComposer>()
                .AddTransient<IComposer, DockerComposer>()

                // Cooks - .NET
                .AddTransient<ICook, DotNetCleanCook>()
                .AddTransient<ICook, DotNetBuildCook>()
                .AddTransient<ICook, DotNetRestoreCook>()
                .AddTransient<ICook, DotNetTestCook>()
                .AddTransient<ICook, DotNetPackCook>()
                .AddTransient<ICook, DotNetNuGetPushCook>()
                .AddTransient<ICook, DotNetPublishCook>()
                .AddTransient<ICook, DotNetDockerFileCook>()
                // Cooks - Docker
                .AddTransient<ICook, DockerBuildCook>()

                // Commands
                .AddTransient<ApplyCommand>()
                .AddTransient<PlanCommand>()
                .AddTransient<RunCommand>();

            return serviceCollection;
        }
    }
}
