using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cookbooks.Recipes;
using Bake.Core;
using Bake.Extensions;
using Bake.Options;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bake
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var parser = new Parser(s =>
                {
                    s.CaseInsensitiveEnumValues = true;
                    s.AutoHelp = true;
                    s.CaseSensitive = false;
                    s.ParsingCulture = CultureInfo.InvariantCulture;
                });

            return parser.ParseArguments<CommandLineOptions>(args)
                .MapResult(
                    o => Main(o).Result,
                    _ => 1);
        }

        public static async Task<int> Main(CommandLineOptions options)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        [$"log:{nameof(LogOptions.Level)}"] = options.LogLevel.ToString()
                    })
                .AddEnvironmentVariables("BAKE_")
                .Build();

            var logOptions = new LogOptions();
            configuration.Bind(logOptions);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(logOptions.Level)
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
