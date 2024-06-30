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
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetDockerFileCook : Cook<DotNetDockerFileRecipe>
    {
        private readonly IDotNetTfmParser _dotNetTfmParser;
        private readonly IDockerLabels _dockerLabels;

        private const string Dockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:{{VERSION}}-jammy-chiseled-extra

{{LABELS}}

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1
ENV DOTNET_GENERATE_ASPNET_CERTIFICATE=0

# OPT OUT OF Diagnostic pipeline so we can run readonly.
ENV COMPlus_EnableDiagnostics=0
ENV DOTNET_EnableDiagnostics=0

WORKDIR /app
COPY ./{{PATH}} .

# Add dumb-init
ADD --chmod=0555 https://github.com/Yelp/dumb-init/releases/download/v{{DUMB_INIT_VERSION}}/dumb-init_{{DUMB_INIT_VERSION}}_x86_64 /usr/bin/dumb-init

ENTRYPOINT [""/usr/bin/dumb-init"", ""--""]

CMD [""dotnet"", ""{{NAME}}""]
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

            var directoryPath = Path.GetDirectoryName(recipe.ProjectPath)!;
            var dockerFilePath = Path.Combine(directoryPath, "Dockerfile");
            var labels = _dockerLabels.Serialize(recipe.Labels);
            
            var dockerfileContent = Dockerfile
                .Replace("{{PATH}}", recipe.ServicePath)
                .Replace("{{NAME}}", recipe.EntryPoint)
                .Replace("{{VERSION}}", $"{targetFrameworkVersion!.Version.Major}.{targetFrameworkVersion.Version.Minor}")
                .Replace("{{DUMB_INIT_VERSION}}", "1.2.5")
                .Replace("{{LABELS}}", labels);

            await File.WriteAllTextAsync(
                dockerFilePath,
                dockerfileContent,
                cancellationToken);

            return true;
        }
    }
}
