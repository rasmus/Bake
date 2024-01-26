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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.ValueObjects.Recipes.Go;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Cooks.Go
{
    public class GoDockerFileCook : Cook<GoDockerFileRecipe>
    {
        private readonly ILogger<GoDockerFileCook> _logger;
        private readonly IDockerLabels _dockerLabels;

        private const string Dockerfile = @"
FROM gcr.io/distroless/base-debian10

{{LABELS}}

WORKDIR /
COPY {{SRC}} /{{DST}}

EXPOSE {{PORT}}

USER nonroot:nonroot
ENTRYPOINT [""/{{DST}}""]
";

        public GoDockerFileCook(
            ILogger<GoDockerFileCook> logger,
            IDockerLabels dockerLabels)
        {
            _logger = logger;
            _dockerLabels = dockerLabels;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            GoDockerFileRecipe recipe,
            CancellationToken cancellationToken)
        {
            var dockerFilePath = Path.Combine(recipe.ProjectPath, "Dockerfile");
            var dockerLabels = _dockerLabels.Serialize(recipe.Labels);
            
            var dockerfileContent = Dockerfile
                .Replace("{{SRC}}", recipe.Output.Replace("\\", "/"))
                .Replace("{{DST}}", Path.GetFileName(recipe.Output))
                .Replace("{{PORT}}", recipe.Port.ToString())
                .Replace("{{LABELS}}", dockerLabels);

            _logger.LogInformation(
                "Creating Dockerfile for Go service at {FilePath} with content {DockerfileContent}",
                dockerFilePath,
                dockerfileContent);

            await File.WriteAllTextAsync(
                dockerFilePath,
                dockerfileContent,
                cancellationToken);

            return true;
        }
    }
}
