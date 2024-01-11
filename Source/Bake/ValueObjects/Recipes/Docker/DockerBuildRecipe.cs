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

using Bake.ValueObjects.Artifacts;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.Docker
{
    [Recipe(Names.Recipes.Docker.Build)]
    public class DockerBuildRecipe : Recipe
    {
        [YamlMember]
        public string WorkingDirectory { get; [Obsolete] set; } = null!;

        [YamlMember]
        public string Name { get; [Obsolete] set; } = null!;

        [YamlMember]
        public string[] Tags { get; [Obsolete] set; } = null!;

        [YamlMember]
        public bool Compress { get; [Obsolete] set; }

        [YamlMember]
        public Dictionary<string, string> SecretMounts { get; [Obsolete] set; } = null!;

        [Obsolete]
        public DockerBuildRecipe() { }

        public DockerBuildRecipe(
            string workingDirectory,
            string name,
            IEnumerable<string> tags,
            bool compress,
            Dictionary<string, string> secretMounts,
            params Artifact[] artifacts)
            : base(artifacts)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            WorkingDirectory = workingDirectory;
            Name = name;
            Tags = tags.ToArray();
            Compress = compress;
            SecretMounts = secretMounts;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
