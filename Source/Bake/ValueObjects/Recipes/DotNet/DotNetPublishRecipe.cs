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

using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.DotNet
{
    [Recipe(RecipeNames.DotNet.Publish)]
    public class DotNetPublishRecipe : Recipe
    {
        [YamlMember(Alias = "path")]
        public string Path { get; }

        [YamlMember(Alias = "publish-single-file")]
        public bool PublishSingleFile { get; }

        [YamlMember(Alias = "self-contained")]
        public bool SelfContained { get; }

        [YamlMember(Alias = "build")]
        public bool Build { get; }

        [YamlMember(Alias = "configuration")]
        public string Configuration { get; }

        [YamlMember(Alias = "runtime")]
        public DotNetTargetRuntime Runtime { get; }

        public DotNetPublishRecipe(
            string path,
            bool publishSingleFile,
            bool selfContained,
            bool build,
            string configuration,
            DotNetTargetRuntime runtime)
        {
            Path = path;
            PublishSingleFile = publishSingleFile;
            SelfContained = selfContained;
            Build = build;
            Configuration = configuration;
            Runtime = runtime;
        }
    }
}
