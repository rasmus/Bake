// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Cooks;
using Bake.Extensions;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
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

                var recipeName = cook.GetName(recipe);

                _logger.LogInformation("Starting cook {RecipeName}", recipeName);

                // TODO: Gather logs

                var stopwatch = Stopwatch.StartNew();
                var success = await cook.CookAsync(
                    context,
                    recipe,
                    cancellationToken);

                if (recipe.Artifacts != null && recipe.Artifacts.Any())
                {
                    _logger.LogDebug("Checking artifacts for {RecipeName}", recipeName);
                    var errors = await recipe.Artifacts
                        .Select(a => a.ValidateAsync(cancellationToken))
                        .SelectManyAsync();

                    if (errors.Any())
                    {
                        _logger.LogError(
                            "Recipe {RecipeName} failed with artifact validation errors: {ArtifactValidationErrors}",
                            recipeName,
                            errors);
                        success = false;
                    }
                }

                cookResults.Add(new CookResult(
                    recipeName,
                    stopwatch.Elapsed,
                    success,
                    recipe.Artifacts));

                if (success)
                {
                    continue;
                }

                successful = false;
                break;
            }

            PrintHeader("artifacts");
            PrintArtifacts(cookResults);
            PrintHeader("timings");
            PrintTimings(cookResults);
            Console.WriteLine();

            return successful;
        }

        private static void PrintHeader(string header)
        {
            Console.WriteLine();
            Console.WriteLine($"{header.ToUpperInvariant()} {new string('=', 79 - header.Length)}");
        }

        private static void PrintTimings(List<CookResult> cookResults)
        {
            var totalSeconds = cookResults.Sum(r => r.Time.TotalSeconds);
            foreach (var cookResult in cookResults)
            {
                var status = cookResult.Success
                    ? "success"
                    : "failed";
                var percent = cookResult.Time.TotalSeconds / totalSeconds;
                var barWidth = (int) Math.Round(percent * BarWidth, MidpointRounding.AwayFromZero);
                Console.WriteLine(
                    $"[{new string('#', barWidth),BarWidth}] {percent * 100.0,5:0.0}%  {cookResult.Name,-32} {status,7} {cookResult.Time.TotalSeconds,6:0.##} seconds");
            }

            Console.WriteLine($"total {totalSeconds:0.##} seconds");
        }

        private static void PrintArtifacts(IEnumerable<CookResult> cookResults)
        {
            var groupedArtifacts = cookResults
                .SelectMany(r => r.Artifacts ?? Enumerable.Empty<Artifact>())
                .GroupBy(a =>
                {
                    var type = a.GetType();
                    var attribute = type.GetCustomAttribute<ArtifactAttribute>();
                    return attribute == null
                        ? throw new InvalidOperationException(
                            $"Artifact type '{type.PrettyPrint()}' is missing the required attribute")
                        : attribute.Name;
                });

            foreach (var artifactGroup in groupedArtifacts)
            {
                var prettyNames = artifactGroup
                    .SelectMany(a => a.PrettyNames())
                    .OrderBy(s => s)
                    .ToArray();

                if (!prettyNames.Any())
                {
                    continue;
                }

                Console.WriteLine(artifactGroup.Key);
                foreach (var prettyName in prettyNames)
                {
                    Console.WriteLine($"  {prettyName}");
                }
            }
        }
    }
}
