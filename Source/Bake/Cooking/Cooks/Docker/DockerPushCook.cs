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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Services;
using Bake.Services.Tools;
using Bake.Services.Tools.DockerArguments;
using Bake.ValueObjects;
using Bake.ValueObjects.Recipes.Docker;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Cooks.Docker
{
    public class DockerPushCook : Cook<DockerPushRecipe>
    {
        private readonly ILogger<DockerPushCook> _logger;
        private readonly IDocker _docker;
        private readonly IContainerTagParser _containerTagParser;
        private readonly ICredentials _credentials;

        public DockerPushCook(
            ILogger<DockerPushCook> logger,
            IDocker docker,
            IContainerTagParser containerTagParser,
            ICredentials credentials)
        {
            _logger = logger;
            _docker = docker;
            _containerTagParser = containerTagParser;
            _credentials = credentials;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            DockerPushRecipe recipe,
            CancellationToken cancellationToken)
        {
            var containerTags = new List<ContainerTag>();
            foreach (var tag in recipe.Tags.Where(t => !t.StartsWith("bake.local/")))
            {
                if (!_containerTagParser.TryParse(tag, out var containerTag))
                {
                    _logger.LogCritical("Failed to parse {ContainerTag}", tag);
                    return false;
                }

                containerTags.Add(containerTag);
            }

            var dockerLogins = (await Task.WhenAll(containerTags
                .Select(t => _credentials.TryGetDockerLoginAsync(t, cancellationToken))))
                .Where(l => l != null)
                .Distinct()
                .ToArray();

            foreach (var dockerLogin in dockerLogins)
            {
                var argument = new DockerLoginArgument(dockerLogin);
                using var toolResult = await _docker.LoginAsync(
                    argument,
                    cancellationToken);

                if (!toolResult.WasSuccessful)
                {
                    return false;
                }
            }

            foreach (var containerTag in containerTags)
            {
                var argument = new DockerPushArgument(containerTag);

                using var toolResult = await _docker.PushAsync(
                    argument,
                    cancellationToken);

                if (!toolResult.WasSuccessful)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
