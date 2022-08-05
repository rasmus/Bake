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
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetDockerFileCook : Cook<DotNetDockerFileRecipe>
    {
        private readonly IDotNetTfmParser _dotNetTfmParser;
        private readonly IDockerLabels _dockerLabels;

        private const string Dockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:{{VERSION}}-alpine

{{LABELS}}

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# OPT OUT OF Diagnostic pipeline so we can run readonly.
ENV COMPlus_EnableDiagnostics=0

WORKDIR /app
COPY ./{{PATH}} .

RUN \
    addgroup -S -g 2000 app_group && \
    adduser \  
        -S \
        -s /sbin/nologin \
        -g app_group \
        app_user && \
    chown app_user:app_group /app

USER app_user:app_group

ENTRYPOINT [""dotnet"", ""{{NAME}}""]
";

        public DotNetDockerFileCook(
            IDotNetTfmParser dotNetTfmParser,
            IDockerLabels dockerLabels)
        {
            _dotNetTfmParser = dotNetTfmParser;
            _dockerLabels = dockerLabels;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DotNetDockerFileRecipe recipe,
            CancellationToken cancellationToken)
        {
            if (!_dotNetTfmParser.TryParse(recipe.Moniker, out var targetFrameworkVersion))
            {
                return false;
            }

            var directoryPath = Path.GetDirectoryName(recipe.ProjectPath);
            var dockerFilePath = Path.Combine(directoryPath, "Dockerfile");
            var labels = _dockerLabels.Serialize(recipe.Labels);
            
            var dockerfileContent = Dockerfile
                .Replace("{{PATH}}", recipe.ServicePath)
                .Replace("{{NAME}}", recipe.EntryPoint)
                .Replace("{{VERSION}}", $"{targetFrameworkVersion.Version.Major}.{targetFrameworkVersion.Version.Minor}")
                .Replace("{{LABELS}}", labels);

            await File.WriteAllTextAsync(
                dockerFilePath,
                dockerfileContent,
                cancellationToken);

            return true;
        }
    }
}
