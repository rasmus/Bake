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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.Go;

namespace Bake.Cooking.Composers
{
    public class GoComposer : Composer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGoModParser _goModParser;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.ToolWindows,
                ArtifactType.ToolLinux,
                ArtifactType.Dockerfile,
            };

        public GoComposer(
            IFileSystem fileSystem,
            IGoModParser goModParser)
        {
            _fileSystem = fileSystem;
            _goModParser = goModParser;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var goModFilePaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "go.mod",
                cancellationToken);

            return (await Task.WhenAll(
                    goModFilePaths
                        .Select(Path.GetDirectoryName)
                        .Select(p => CreateRecipesAsync(p, cancellationToken))))
                .SelectMany(l => l)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<Recipe>> CreateRecipesAsync(
            string directoryPath,
            CancellationToken cancellationToken)
        {
            var goModPath = Path.Combine(directoryPath, "go.mod");
            var goModContent = await _fileSystem.ReadAllTextAsync(
                goModPath,
                cancellationToken);

            if (!_goModParser.TryParse(goModContent, out var goModuleName))
            {
                throw new InvalidOperationException($"Invalid content of '{goModPath}'");
            }

            var windowsOutput = $"{goModuleName.Name}.exe";

            var recipes = new Recipe[]
                {
                    new GoTestRecipe(
                        directoryPath),
                    new GoBuildRecipe(
                        windowsOutput,
                        directoryPath,
                        GoOs.Windows,
                        GoArch.AMD64)
                };

            return recipes;
        }
    }
}
