// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using Bake.Services.Tools.NPMArguments;
using Bake.ValueObjects.Recipes.NodeJS;

namespace Bake.Cooking.Cooks.NodeJS
{
    public class NpmCICook : Cook<NpmCIRecipe>
    {
        private readonly INPM _npm;

        public NpmCICook(
            INPM npm)
        {
            _npm = npm;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            NpmCIRecipe recipe,
            CancellationToken cancellationToken)
        {
            var argument = new NPMCIArgument(
                recipe.WorkingDirectory);

            var toolResult = await _npm.CIAsync(
                argument,
                cancellationToken);

            return toolResult.WasSuccessful;
        }
    }
}
