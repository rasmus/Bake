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

using Bake.Core;
using Bake.ValueObjects.Artifacts;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    [Recipe(Names.Recipes.DotNet.Pack)]
    public class DotNetPackProjectRecipe : Recipe
    {
        [YamlMember]
        public string Path { get; [Obsolete] set; } = null!;

        [YamlMember]
        public bool Restore { get; [Obsolete] set; }

        [YamlMember]
        public bool Build { get; [Obsolete] set; }

        [YamlMember]
        public bool IncludeSymbols { get; [Obsolete] set; }

        [YamlMember]
        public bool IncludeSource { get; [Obsolete] set; }

        [YamlMember]
        public string Configuration { get; [Obsolete] set; } = null!;

        [YamlMember]
        public SemVer Version { get; [Obsolete] set; } = null!;

        [YamlMember]
        public Dictionary<string, string> Properties { get; [Obsolete] set; } = null!;

        [Obsolete]
        public DotNetPackProjectRecipe() { }

        public DotNetPackProjectRecipe(
            string path,
            bool restore,
            bool build,
            bool includeSymbols,
            bool includeSource,
            string configuration,
            SemVer version,
            Dictionary<string, string> properties,
            params Artifact[] artifacts)
            : base(artifacts)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Path = path;
            Restore = restore;
            Build = build;
            IncludeSymbols = includeSymbols;
            IncludeSource = includeSource;
            Configuration = configuration;
            Version = version;
            Properties = properties;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public override string ToString()
        {
            return $".NET pack {Path}";
        }
    }
}
