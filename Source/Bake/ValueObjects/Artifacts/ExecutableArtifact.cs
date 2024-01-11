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

using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Artifacts
{
    [Artifact(Names.Artifacts.ExecutableArtifact)]
    public class ExecutableArtifact : FileArtifact
    {
        [YamlMember]
        public string Name { get; [Obsolete] set; } = null!;

        [YamlMember]
        public Platform Platform { get; [Obsolete] set; } = null!;

        [Obsolete]
        public ExecutableArtifact() { }

        public ExecutableArtifact(
            string name,
            string path,
            Platform platform)
            : base(path)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Name = name;
            Platform = platform;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public override IEnumerable<string> PrettyNames()
        {
            var relativePath = System.IO.Path.GetRelativePath(
                Directory.GetCurrentDirectory(),
                Path);
            var filename = System.IO.Path.GetFileName(Path);

            yield return $"{filename} ({Platform} {relativePath})";
        }
    }
}
