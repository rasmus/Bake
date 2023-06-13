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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Helm;
using YamlDotNet.Serialization;

namespace Bake.Cooking.Composers
{
    public class HelmComposer : Composer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IYaml _yaml;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
        {
            ArtifactType.HelmChart
        };

        public HelmComposer(
            IFileSystem fileSystem,
            IYaml yaml)
        {
            _fileSystem = fileSystem;
            _yaml = yaml;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var chartFiles = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "Chart.yaml",
                cancellationToken);

            var ingredients = context.Ingredients;

            return (await Task.WhenAll(chartFiles
                    .Select(p => CreateRecipes(p, ingredients, cancellationToken))))
                .SelectMany(r => r)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<Recipe>> CreateRecipes(
            string chartFilePath,
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            var recipes = new List<Recipe>();

            var yaml = await _fileSystem.ReadAllTextAsync(chartFilePath, cancellationToken);
            var chart = await _yaml.DeserializeAsync<HelmChart>(
                yaml,
                cancellationToken);

            var chartDirectory = Path.GetDirectoryName(chartFilePath);
            var chartFileName = $"{chart.Name}-{ingredients.Version}.tgz";
            var parentDirectory = Path.GetDirectoryName(chartDirectory);

            recipes.Add(new HelmLintRecipe(
                chartDirectory));
            recipes.Add(new HelmPackageRecipe(
                chartDirectory,
                parentDirectory,
                ingredients.Version,
                new HelmChartArtifact(
                    Path.Combine(parentDirectory, chartFileName))));

            return recipes;
        }

        private class HelmChart
        {
            [YamlMember(Alias = "name")]
            public string Name { get; set; }
        }
    }
}
