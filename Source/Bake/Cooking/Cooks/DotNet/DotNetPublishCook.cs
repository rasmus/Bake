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

using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Services.DotNetArgumentBuilders;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetPublishCook : Cook<DotNetPublishRecipe>
    {
        private readonly IDotNet _dotNet;
        public override string RecipeName => RecipeNames.DotNet.Publish;

        public DotNetPublishCook(
            IDotNet dotNet)
        {
            _dotNet = dotNet;
        }

        protected override Task<bool> CookAsync(
            IContext context,
            DotNetPublishRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new DotNetPublishArgument(
                recipe.Path,
                recipe.PublishSingleFile,
                recipe.SelfContained,
                recipe.Build,
                recipe.Configuration,
                recipe.Runtime);

            return _dotNet.PublishAsync(
                argument,
                cancellationToken);
        }
    }
}