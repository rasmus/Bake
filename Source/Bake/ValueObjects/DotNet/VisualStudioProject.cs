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

namespace Bake.ValueObjects.DotNet
{
    public class VisualStudioProject
    {
        public string Path { get; }
        public CsProj CsProj { get; }
        public string Directory { get; }
        public string Name { get; set; }
        public string AssemblyName => string.IsNullOrEmpty(CsProj.AssemblyName) ? Name : CsProj.AssemblyName;

        public bool ShouldBePacked => CsProj.IsPackable || CsProj.PackAsTool;
        public bool ShouldBePushed => CsProj.IsPackable || CsProj.PackAsTool;

        public VisualStudioProject(
            string path,
            CsProj csProj)
        {
            Path = path;
            CsProj = csProj;
            Directory = System.IO.Path.GetDirectoryName(path);
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
