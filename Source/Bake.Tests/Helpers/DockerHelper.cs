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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            string image,
            IReadOnlyDictionary<int, int> ports,
            IReadOnlyDictionary<string, string> environmentVariables,
            CancellationToken cancellationToken)
        {
            var createContainerResponse = await Client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = image,
                    ExposedPorts = ports.Keys.ToDictionary(
                        p => p.ToString(),
                        _ => default(EmptyStruct)),
                    Env = environmentVariables
                        .Select(kv => $"{kv.Key}={kv.Value}")
                        .ToList(),
                    AttachStdout = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = ports.ToDictionary(
                            kv => kv.Key.ToString(),
                            kv => (IList<PortBinding>) new List<PortBinding>{new() {HostPort = kv.Value.ToString()}}),
                        PublishAllPorts = true,
                    },
                },
                cancellationToken);

            await Client.Containers.StartContainerAsync(
                createContainerResponse.ID,
                null,
                cancellationToken);

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
    }
}
