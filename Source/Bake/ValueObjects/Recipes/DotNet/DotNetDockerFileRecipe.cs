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
using Bake.ValueObjects.Artifacts;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    [Recipe(Names.Recipes.DotNet.DockerFile)]
    public class DotNetDockerFileRecipe : Recipe
    {
        [YamlMember]
        public string ProjectPath { get; [Obsolete] set; }

        [YamlMember]
        public string ServicePath { get; [Obsolete] set; }

        [YamlMember]
        public string EntryPoint { get; [Obsolete] set; }

        [YamlMember]
        public string Moniker { get; [Obsolete] set; }

        [Obsolete]
        public DotNetDockerFileRecipe() { }

        public DotNetDockerFileRecipe(
            string projectPath,
            string servicePath,
            string entryPoint,
            string moniker,
            params Artifact[] artifacts)
            : base(artifacts)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            ProjectPath = projectPath;
            ServicePath = servicePath;
            EntryPoint = entryPoint;
            Moniker = moniker;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
