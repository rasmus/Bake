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

using Bake.Core;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Artifacts
{
    [Artifact(Names.Artifacts.ReleaseArtifact)]
    public class ReleaseArtifact : Artifact
    {
        [YamlMember]
        public SemVer Version { get; [Obsolete] set; } = null!;

        [YamlMember]
        public ReleaseNotes? ReleaseNotes { get; [Obsolete] set; }

        [YamlMember]
        public ReleaseFileArtifact[] Files { get; [Obsolete] set; } = null!;

        [Obsolete]
        public ReleaseArtifact() { }

        public ReleaseArtifact(
            SemVer version,
            ReleaseNotes? releaseNotes,
            ReleaseFileArtifact[] files)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Version = version;
            ReleaseNotes = releaseNotes;
            Files = files;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public override IAsyncEnumerable<string> ValidateAsync(CancellationToken cancellationToken)
        {
            return AsyncEnumerable.Empty<string>();
        }

        public override IEnumerable<string> PrettyNames()
        {
            yield return "release";
        }
    }
}
