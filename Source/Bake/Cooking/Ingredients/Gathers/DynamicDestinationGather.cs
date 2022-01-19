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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Destinations;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class DynamicDestinationGather : IGather
    {
        private readonly ILogger<DynamicDestinationGather> _logger;
        private readonly IDefaults _defaults;

        public DynamicDestinationGather(
            ILogger<DynamicDestinationGather> logger,
            IDefaults defaults)
        {
            _logger = logger;
            _defaults = defaults;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            var dynamicDestinations = ingredients.Destinations
                .OfType<DynamicDestination>()
                .ToList();

            if (!dynamicDestinations.Any())
            {
                return;
            }

            foreach (var dynamicDestination in dynamicDestinations)
            {
                if (!Names.ArtifactTypes.TryGetType(dynamicDestination.ArtifactType, out var artifactType))
                {
                    throw new ArgumentOutOfRangeException(
                        $"Unknown artifact type {dynamicDestination.ArtifactType}");
                }

                _logger.LogDebug(
                    "Investigating dynamic destination for {ArtifactType} to {DynamicDestination}",
                    dynamicDestination.ArtifactType,
                    dynamicDestination.Destination);

                switch (artifactType)
                {
                    case ArtifactType.NuGet:
                        await ExtractNuGetDestinationAsync(ingredients, dynamicDestination);
                        break;

                    case ArtifactType.Container:
                        await ExtractContainerDestinationAsync(ingredients, dynamicDestination);
                        break;

                    case ArtifactType.Release:
                        await ExtractReleaseDestinationAsync(ingredients, dynamicDestination);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task ExtractReleaseDestinationAsync(
            ValueObjects.Ingredients ingredients,
            DynamicDestination dynamicDestination)
        {
            switch (dynamicDestination.Destination)
            {
                case Names.DynamicDestinations.GitHub:
                    var gitHubInformation = await ingredients.GitHubTask;

                    var gitHubReleaseDestination = new GitHubReleaseDestination(
                        gitHubInformation.Owner,
                        gitHubInformation.Repository);
                    ingredients.Destinations.Add(gitHubReleaseDestination);
                    ingredients.Destinations.Remove(dynamicDestination);

                    _logger.LogDebug(
                        "Updated dynamic destination for {ArtifactType} from {DynamicDestination} to {StaticDestination}",
                        dynamicDestination.ArtifactType,
                        dynamicDestination.Destination,
                        gitHubReleaseDestination.ToString());

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task ExtractContainerDestinationAsync(
            ValueObjects.Ingredients ingredients,
            DynamicDestination dynamicDestination)
        {
            switch (dynamicDestination.Destination)
            {
                case Names.DynamicDestinations.GitHub:
                    var gitHubInformation = await ingredients.GitHubTask;
                    var tag = _defaults.GitHubUserRegistry.Replace("{USER}", gitHubInformation.Owner);

                    var containerRegistryDestination = new ContainerRegistryDestination(tag);
                    ingredients.Destinations.Add(containerRegistryDestination);
                    ingredients.Destinations.Remove(dynamicDestination);

                    _logger.LogDebug(
                        "Updated dynamic destination for {ArtifactType} from {DynamicDestination} to {StaticDestination}",
                        dynamicDestination.ArtifactType,
                        dynamicDestination.Destination,
                        containerRegistryDestination.Url);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task ExtractNuGetDestinationAsync(
            ValueObjects.Ingredients ingredients,
            DynamicDestination dynamicDestination)
        {
            switch (dynamicDestination.Destination)
            {
                case Names.DynamicDestinations.GitHub:
                    var gitHubInformation = await ingredients.GitHubTask;
                    var url = new Uri(_defaults.GitHubNuGetRegistry.Replace("/OWNER/", $"/{gitHubInformation.Owner}/"), UriKind.Absolute);

                    var nugetRegistryDestination = new NuGetRegistryDestination(url);
                    ingredients.Destinations.Add(nugetRegistryDestination);
                    ingredients.Destinations.Remove(dynamicDestination);

                    _logger.LogDebug(
                        "Updated dynamic destination for {ArtifactType} from {DynamicDestination} to {StaticDestination}",
                        dynamicDestination.ArtifactType,
                        dynamicDestination.Destination,
                        nugetRegistryDestination.Url);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
