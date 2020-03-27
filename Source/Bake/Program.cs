using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cookbooks.Recipes;
using Bake.Core;
using Bake.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bake
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("BAKE_")
                .Build();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var singletons = new []
                {
                    typeof(State)
                };

            var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddOptions()
                .AddLogging(b => b
                    .AddSerilog(logger))
                .ScanByConvention<Program>(singletons);

            using (var cts = new CancellationTokenSource())
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var recipe = serviceProvider.GetRequiredService<IRecipe>();
                await recipe.BakeAsync(
                    Directory.GetCurrentDirectory(),
                    cts.Token);
            }

            return 0;
        }
    }
}
