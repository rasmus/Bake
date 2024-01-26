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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Bake.Tests.Helpers
{
    public static class DockerHelper
    {
        private static readonly DockerClient Client = new DockerClientConfiguration().CreateClient();

        public static async Task<IDisposable> RunAsync(
            DockerArguments arguments,
            CancellationToken cancellationToken)
        {
            var createContainerResponse = await Client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = arguments.Image,
                    ExposedPorts = arguments.Ports.Keys.ToDictionary(
                        p => p.ToString(),
                        _ => default(EmptyStruct)),
                    Env = arguments.EnvironmentVariables
                        .Select(kv => $"{kv.Key}={kv.Value}")
                        .ToList(),
                    AttachStdout = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = arguments.Ports.ToDictionary(
                            kv => kv.Key.ToString(),
                            kv => (IList<PortBinding>) new List<PortBinding>{new()
                                {
                                    HostPort = kv.Value.ToString(),
                                }}),
                        PublishAllPorts = true,
                        ReadonlyRootfs = arguments.ReadOnlyFilesystem,
                    },
                },
                cancellationToken);

            await Client.Containers.StartContainerAsync(
                createContainerResponse.ID,
                null,
                cancellationToken);

            // TODO: What to do here?
            var _ = Task.Run(async () =>
            {
                await Client.Containers.GetContainerLogsAsync(
                    createContainerResponse.ID,
                    new ContainerLogsParameters
                    {
                        Follow = true,
                        ShowStderr = true,
                        ShowStdout = true,
                        Since = "0",
                    },
                    cancellationToken,
                    new ConsoleOutProgress(arguments.Image.Split('/').Last()));
            });

            return new DisposableAction(() =>
            {
                Client.Containers.StopContainerAsync(
                    createContainerResponse.ID,
                    new ContainerStopParameters
                    {
                        WaitBeforeKillSeconds = 30,
                    },
                    CancellationToken.None).Wait(CancellationToken.None);
                Client.Containers.RemoveContainerAsync(
                    createContainerResponse.ID,
                    new ContainerRemoveParameters
                    {
                        Force = true
                    },
                    CancellationToken.None).Wait(CancellationToken.None);
            });
        }

        public static async Task<IReadOnlyCollection<string>> ListImagesAsync(
            CancellationToken cancellationToken = default)
        {
            var imageResponse = await Client.Images.ListImagesAsync(
                new ImagesListParameters
                {
                    All = true,
                },
                cancellationToken);

            return imageResponse
                .Where(i => i.RepoTags != null)
                .SelectMany(i => i.RepoTags)
                .OrderBy(t => t)
                .ToArray();
        }

        private class ConsoleOutProgress : IProgress<string>
        {
            private static readonly Regex UnicodeFinder = new(
                @"[^\t\r\n -~]",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private readonly string _prefix;

            public ConsoleOutProgress(
                string prefix)
            {
                _prefix = prefix;
            }

            public void Report(string value)
            {
                value = UnicodeFinder.Replace(value, string.Empty);
                Console.WriteLine($"{_prefix}: {value}");
            }
        }
    }
}
