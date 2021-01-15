using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Cooks;
using Bake.Extensions;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking
{
    public class Kitchen : IKitchen
    {
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

        public async Task CookAsync(
            IContext context,
            Book book,
            CancellationToken cancellationToken)
        {
            foreach (var recipe in book.Recipes)
            {
                var recipeType = recipe.GetType();
                if (!_cooks.TryGetValue(recipe.GetType(), out var cook))
                {
                    throw new Exception($"Don't know how to cook {recipeType.PrettyPrint()}");
                }

                _logger.LogInformation("Starting cook {CookName}", cook.Name);

                await cook.CookAsync(
                    context,
                    recipe,
                    cancellationToken);
            }
        }
    }
}
