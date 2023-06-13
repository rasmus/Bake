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

namespace Bake.Tests.Helpers
{
    public class DockerArguments
    {
        public static DockerArguments With(string image) => new(image);

        private readonly Dictionary<int, int> _ports = new();
        private readonly Dictionary<string, string> _environmentVariables = new();

        public string Image { get; }
        public bool ReadOnlyFilesystem { get; private set; }
        public IReadOnlyDictionary<int, int> Ports => _ports;
        public IReadOnlyDictionary<string, string> EnvironmentVariables => _environmentVariables;

        private DockerArguments(
            string image)
        {
            Image = image;
        }

        public DockerArguments WithPort(int containerPort)
        {
            return WithPort(containerPort, SocketHelper.FreeTcpPort());
        }

        public DockerArguments WithPort(int containerPort, int hostPort)
        {
            _ports[containerPort] = hostPort;
            return this;
        }

        public DockerArguments WithEnvironmentVariable(string name, string value)
        {
            _environmentVariables[name] = value;
            return this;
        }

        public DockerArguments WithReadOnlyFilesystem()
        {
            ReadOnlyFilesystem = true;
            return this;
        }
    }
}
