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

using System.Threading;
using System.Threading.Tasks;
using Bake.Services.Tools;
using Bake.Services.Tools.HelmArguments;
using Bake.ValueObjects.Recipes.Helm;

namespace Bake.Cooking.Cooks.Helm
{
    public class HelmPackageCook : Cook<HelmPackageRecipe>
    {
        private readonly IHelm _helm;

        public HelmPackageCook(
            IHelm helm)
        {
            _helm = helm;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            HelmPackageRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new HelmPackageArgument(
                recipe.ChartDirectory,
                recipe.OutputDirectory,
                recipe.Version);

            var toolResult = await _helm.PackageAsync(
                argument,
                cancellationToken);

            return toolResult.WasSuccessful;
        }
    }
}
