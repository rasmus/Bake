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
using Bake.ValueObjects.Recipes.NodeJS;
using File = System.IO.File;

// ReSharper disable StringLiteralTypo

namespace Bake.Cooking.Cooks.NodeJS
{
    public class NodeJSDockerfileCook : Cook<NodeJSDockerfileRecipe>
    {
        private readonly IDockerLabels _dockerLabels;

        private const string Dockerfile = @"
FROM node:lts-alpine3.15

ENV NODE_ENV production

{{LABELS}}

USER node
WORKDIR /usr/src/app

# Do a clean restore first
COPY --chown=node:node ./package*.json /usr/src/app/
RUN \
    {{NPMRC_MOUNT}} npm ci --only=production && \
    npm cache clean --force && \
    rm -f ./package*.json

# Copy service
COPY --chown=node:node . /usr/src/app/

# Add dumb-init
ADD --chown=node:node https://github.com/Yelp/dumb-init/releases/download/v{{DUMB_INIT_VERSION}}/dumb-init_{{DUMB_INIT_VERSION}}_x86_64 /usr/bin/dumb-init
RUN chmod +x /usr/bin/dumb-init

ENTRYPOINT [""/usr/bin/dumb-init"", ""--""]

CMD [""node"", ""{{MAIN}}""]
";

        public NodeJSDockerfileCook(
            IDockerLabels dockerLabels)
        {
            _dockerLabels = dockerLabels;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            NodeJSDockerfileRecipe recipe,
            CancellationToken cancellationToken)
        {
            var dockerFilePath = Path.Combine(recipe.WorkingDirectory, "Dockerfile");
            var labels = _dockerLabels.Serialize(recipe.Labels);

            var npmRcMount = File.Exists(Path.Combine(recipe.WorkingDirectory, ".npmrc"))
                ? "--mount=type=secret,id=npmrc,target=/usr/src/app/.npmrc"
                : string.Empty;

            var dockerfileContent = Dockerfile
                .Replace("{{LABELS}}", labels)
                .Replace("{{MAIN}}", recipe.Main)
                .Replace("{{DUMB_INIT_VERSION}}", "1.2.5")
                .Replace("{{NPMRC_MOUNT}}", npmRcMount);

            await File.WriteAllTextAsync(
                dockerFilePath,
                dockerfileContent,
                cancellationToken);

            return true;
        }
    }
}
