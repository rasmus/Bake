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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.ValueObjects.Recipes.Python;

namespace Bake.Cooking.Cooks.Python
{
    public class PythonFlaskDockerfileCook : Cook<PythonFlaskDockerfileRecipe>
    {
        private readonly IDockerLabels _dockerLabels;
        private readonly IDockerIgnores _dockerIgnores;

        private const string Dockerfile = @"
FROM python:3.9-alpine

{{LABELS}}

WORKDIR /app
COPY ./ .

RUN \
  adduser -h /app --disabled-password appuser && \
  pip install --no-cache-dir -r requirements.txt

USER appuser
CMD [ ""python"", ""-m"", ""flask"", ""run"", ""--host=0.0.0.0"" ]
";

        public PythonFlaskDockerfileCook(
            IDockerLabels dockerLabels,
            IDockerIgnores dockerIgnores)
        {
            _dockerLabels = dockerLabels;
            _dockerIgnores = dockerIgnores;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            PythonFlaskDockerfileRecipe recipe,
            CancellationToken cancellationToken)
        {
            var dockerFilePath = Path.Combine(recipe.Directory, "Dockerfile");
            var labels = _dockerLabels.Serialize(recipe.Labels);

            var dockerfileContent = Dockerfile
                .Replace("{{LABELS}}", labels);

            await File.WriteAllTextAsync(
                dockerFilePath,
                dockerfileContent,
                cancellationToken);

            await _dockerIgnores.WriteAsync(
                recipe.Directory,
                cancellationToken);

            return true;
        }
    }
}
