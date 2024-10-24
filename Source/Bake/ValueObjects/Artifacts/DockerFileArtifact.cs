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

using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Artifacts
{
    [Artifact(Names.Artifacts.DockerfileArtifact)]
    public class DockerFileArtifact : FileArtifact
    {
        [YamlMember]
        public string Name { get; [Obsolete] set; } = null!;

        [YamlMember]
        public Dictionary<string, string> Labels { get; [Obsolete] set; } = null!;

        [Obsolete]
        public DockerFileArtifact() { }

        public DockerFileArtifact(
            string name,
            string path,
            Dictionary<string, string> labels)
            : base(path)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Name = name;
            Labels = labels;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public override IEnumerable<string> PrettyNames()
        {
            return Enumerable.Empty<string>();
        }
    }
}
