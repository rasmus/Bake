using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Cooks;
using Bake.Extensions;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking
{
    public class Kitchen : IKitchen
    {
        private const int BarWidth = 10;
        private readonly ILogger<Kitchen> _logger;
        private readonly IReadOnlyDictionary<Type, ICook> _cooks;

        public Kitchen(
            ILogger<Kitchen> logger,
            IEnumerable<ICook> cooks)
        {
            _logger = logger;
            _cooks = cooks
                .ToDictionary(
                    c => c.CanCook,
                    c => c);
        }

        public async Task<bool> CookAsync(
            IContext context,
            Book book,
            CancellationToken cancellationToken)
        {
            var cookResults = new List<CookResult>();
            var successful = true;

            foreach (var recipe in book.Recipes)
            {
                var recipeType = recipe.GetType();
                if (!_cooks.TryGetValue(recipe.GetType(), out var cook))
                {
                    throw new Exception($"Don't know how to cook {recipeType.PrettyPrint()}");
                }

                _logger.LogInformation("Starting cook {CookName}", cook.Name);

                var stopwatch = Stopwatch.StartNew();
                var success = await cook.CookAsync(
                    context,
                    recipe,
                    cancellationToken);

                cookResults.Add(new CookResult(
                    cook.Name,
                    stopwatch.Elapsed,
                    success));

                if (success)
                {
                    continue;
                }

                successful = false;
                break;
            }

            var totalSeconds = cookResults.Sum(r => r.Time.TotalSeconds);
            foreach (var cookResult in cookResults)
            {
                var status = cookResult.Success
                    ? "success"
                    : "failed";
                var percent = cookResult.Time.TotalSeconds / totalSeconds;
                var barWidth = (int)Math.Round(percent * BarWidth, MidpointRounding.AwayFromZero);
                Console.WriteLine($"[{new string('#', barWidth),BarWidth}] {cookResult.Name,-20} {status, 7} {cookResult.Time.TotalSeconds,6:0.##} seconds");
            }
            Console.WriteLine($"total {totalSeconds:0.##} seconds");

            return successful;
        }
    }
}
