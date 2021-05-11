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

                _logger.LogInformation("Starting cook {CookName}", cook.RecipeName);

                var stopwatch = Stopwatch.StartNew();
                var success = await cook.CookAsync(
                    context,
                    recipe,
                    cancellationToken);

                cookResults.Add(new CookResult(
                    cook.RecipeName,
                    stopwatch.Elapsed,
                    success));

                if (success)
                {
                    continue;
                }

                successful = false;
                break;
            }

            Console.WriteLine();
            var totalSeconds = cookResults.Sum(r => r.Time.TotalSeconds);
            foreach (var cookResult in cookResults)
            {
                var status = cookResult.Success
                    ? "success"
                    : "failed";
                var percent = cookResult.Time.TotalSeconds / totalSeconds;
                var barWidth = (int)Math.Round(percent * BarWidth, MidpointRounding.AwayFromZero);
                Console.WriteLine($"[{new string('#', barWidth),BarWidth}] {percent*100.0,5:0.0}%  {cookResult.Name,-20} {status, 7} {cookResult.Time.TotalSeconds,6:0.##} seconds");
            }
            Console.WriteLine($"total {totalSeconds:0.##} seconds");
            Console.WriteLine();

            return successful;
        }
    }
}
