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
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Docker;

namespace Bake.Cooking.Composers
{
    public class DockerComposer : IComposer
    {
        private readonly IFileSystem _fileSystem;

        public DockerComposer(
            IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var dockerFilePaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "Dockerfile",
                cancellationToken);

            var recipes = new List<Recipe>();

            foreach (var dockerFilePath in dockerFilePaths)
            {
                recipes.AddRange(CreateRecipes(dockerFilePath));
            }

            return recipes;
        }

        private static IEnumerable<Recipe> CreateRecipes(
            string path)
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(path));

            yield return new DockerBuildRecipe(
                path,
                directoryName.ToSlug());
        }
    }
}
