// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using System.Reflection;
using Bake.Extensions;
using Bake.ValueObjects.Recipes;

namespace Bake.Cooking.Cooks
{
    public abstract class Cook<T> : ICook
        where T : Recipe
    {
        protected static readonly string Name = ((RecipeAttribute) typeof(T).GetCustomAttribute(typeof(RecipeAttribute))!).Name;

        public Type CanCook { get; } = typeof(T);

        public string GetName(Recipe recipe)
        {
            return GetName(CastRecipe(recipe));
        }

        protected virtual string GetName(T recipe)
        {
            return Name;
        }

        public Task<bool> CookAsync(
            IContext context,
            Recipe recipe,
            CancellationToken cancellationToken)
        {
            return CookAsync(
                context,
                CastRecipe(recipe),
                cancellationToken);
        }

        protected abstract Task<bool> CookAsync(
            IContext context,
            T recipe,
            CancellationToken cancellationToken);

        private static T CastRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (recipe is not T myRecipe)
            {
                throw new ArgumentException(
                    $"I don't know how to cook a '{recipe.GetType().PrettyPrint()}'",
                    nameof(recipe));
            }

            return myRecipe;
        }
    }
}
