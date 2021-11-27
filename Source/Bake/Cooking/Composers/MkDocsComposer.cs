﻿// MIT License
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
using Bake.ValueObjects.Recipes.Pip;
using File = System.IO.File;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Composers
{
    public class MkDocsComposer : Composer
    {
        private readonly IFileSystem _fileSystem;

        public override IReadOnlyCollection<ArtifactType> Produces { get; } = new[]
            {
                ArtifactType.DocumentationSite,
            };

        public MkDocsComposer(
            IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override async Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var mkDocsFilePaths = await _fileSystem.FindFilesAsync(
                context.Ingredients.WorkingDirectory,
                "mkdocs.yml",
                cancellationToken);

            return mkDocsFilePaths
                .SelectMany(CreateRecipes)
                .ToArray();
        }

        private static IEnumerable<Recipe> CreateRecipes(
            string mkDocsFilePath)
        {
            var workingDirectory = Path.GetDirectoryName(mkDocsFilePath);
            var requirementsFilePath = Path.Combine(
                workingDirectory,
                "requirements.txt");

            if (File.Exists(requirementsFilePath))
            {
                yield return new PipInstallRequirementsRecipe(
                    requirementsFilePath,
                    workingDirectory);
            }
        }
    }
}