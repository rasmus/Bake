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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Helm;

namespace Bake.Cooking.Composers
{
    public class HelmComposer : Composer
    {
        private readonly IFileSystem _fileSystem;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
        {
            ArtifactType.HelmChart
        };

        public HelmComposer(
            IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var chartFiles = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "Chart.yaml",
                cancellationToken);

            return chartFiles
                .Select(Path.GetDirectoryName)
                .SelectMany(CreateRecipes)
                .ToList();
        }

        private static IEnumerable<Recipe> CreateRecipes(
            string chartDirectory)
        {
            yield return new HelmLintRecipe(
                chartDirectory);
        }
    }
}
