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

namespace Bake.ValueObjects
{
    public class ContainerTag
    {
        public bool IsDockerHub => string.IsNullOrEmpty(HostAndPort);

        public string HostAndPort { get; }
        public string Path { get; }
        public string Name { get; }
        public string Label { get; }

        public ContainerTag(
            string hostAndPort,
            string path,
            string name,
            string label)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            HostAndPort = (hostAndPort ?? string.Empty).Trim('/');
            Path = (path ?? string.Empty).Trim('/');
            Name = name;
            Label = string.IsNullOrEmpty(label)
                ? "latest"
                : label;
        }

        public override string ToString()
        {
            var completePath = string.IsNullOrEmpty(Path)
                ? Name
                : $"{Path}/{Name}";

            return IsDockerHub
                ? $"{completePath}:{Label}"
                : $"{HostAndPort}/{completePath}:{Label}";
        }
    }
}
