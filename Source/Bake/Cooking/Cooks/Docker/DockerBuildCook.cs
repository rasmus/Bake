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

using Bake.Services;
using Bake.Services.Tools;
using Bake.Services.Tools.DockerArguments;
using Bake.ValueObjects.Recipes.Docker;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Cooks.Docker
{
    public class DockerBuildCook : Cook<DockerBuildRecipe>
    {
        private readonly ILogger<DockerBuildCook> _logger;
        private readonly IDockerIgnores _dockerIgnores;
        private readonly IDocker _docker;

        public DockerBuildCook(
            ILogger<DockerBuildCook> logger,
            IDockerIgnores dockerIgnores,
            IDocker docker)
        {
            _logger = logger;
            _dockerIgnores = dockerIgnores;
            _docker = docker;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DockerBuildRecipe recipe,
            CancellationToken cancellationToken)
        {
            var directoryPath = Path.GetDirectoryName(recipe.WorkingDirectory)!;
            var dockerIgnoreFilePath = Path.Join(directoryPath, ".dockerignore");
            if (!File.Exists(dockerIgnoreFilePath))
            {
                _logger.LogWarning(
                    "There is no '.dockerignore' file at {FilePath}, consider adding one! Bake will create one with some sensible defaults for you",
                    dockerIgnoreFilePath);
                await _dockerIgnores.WriteAsync(
                    directoryPath,
                    cancellationToken);
            }

            var argument = new DockerBuildArgument(
                recipe.WorkingDirectory,
                recipe.Tags,
                recipe.Compress,
                recipe.SecretMounts,
                recipe.Labels);

            using var toolResult = await _docker.BuildAsync(
                argument,
                cancellationToken);

            return toolResult.WasSuccessful;
        }
    }
}
