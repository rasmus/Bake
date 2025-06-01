// MIT License
// 
// Copyright (c) 2021-2025 Rasmus Mikkelsen
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
    public class CsProj
    {
        public bool PackAsTool { get; }
        public string ToolCommandName { get; }
        public string AssemblyName { get; }
        public bool IsPackable { get; }
        public bool IsPublishable { get; }
        public string PackageId { get; }
        public IReadOnlyCollection<TargetFrameworkVersion> TargetFrameworkVersions { get; }
        public bool? IncludeSymbols { get; }

        public CsProj(bool packAsTool,
            string toolCommandName,
            string assemblyName,
            bool isPackable,
            bool isPublishable,
            string packageId,
            IReadOnlyCollection<TargetFrameworkVersion> targetFrameworkVersions,
            bool? includeSymbols)
        {
            PackAsTool = packAsTool;
            ToolCommandName = toolCommandName;
            AssemblyName = assemblyName;
            IsPackable = isPackable;
            IsPublishable = isPublishable;
            PackageId = packageId;
            TargetFrameworkVersions = targetFrameworkVersions;
            IncludeSymbols = includeSymbols;
        }
    }
}
