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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Platform
    {
        public static Platform[] Defaults { get; } = 
            {
                new(ExecutableOperatingSystem.Windows, ExecutableArchitecture.Intel64),
                new(ExecutableOperatingSystem.Linux, ExecutableArchitecture.Intel64),
                new(ExecutableOperatingSystem.MacOSX, ExecutableArchitecture.Intel64),
            };
        public static Platform Any { get; } = new(ExecutableOperatingSystem.Any, ExecutableArchitecture.Any);

        private static readonly IReadOnlyDictionary<ExecutableOperatingSystem, string> DotNetOsMoniker = new ConcurrentDictionary<ExecutableOperatingSystem, string>
            {
                [ExecutableOperatingSystem.Linux] = "linux",
                [ExecutableOperatingSystem.Windows] = "win",
                [ExecutableOperatingSystem.MacOSX] = "osx",
            };
        private static readonly IReadOnlyDictionary<ExecutableArchitecture, string> DotNetArchMoniker = new ConcurrentDictionary<ExecutableArchitecture, string>
            {
                [ExecutableArchitecture.Intel64] = "x64",
            };


        [YamlMember]
        public ExecutableOperatingSystem Os { get; [Obsolete] set; }

        [YamlMember]
        public ExecutableArchitecture Arch { get; [Obsolete] set; }

        [Obsolete]
        public Platform(){}

        public Platform(
            ExecutableOperatingSystem os,
            ExecutableArchitecture arch)
        {
            if (!Enum.IsDefined(typeof(ExecutableOperatingSystem), os))
            {
                throw new ArgumentOutOfRangeException(nameof(os));
            }
            if (!Enum.IsDefined(typeof(ExecutableArchitecture), arch))
            {
                throw new ArgumentOutOfRangeException(nameof(arch));
            }

#pragma warning disable CS0612 // Type or member is obsolete
            Os = os;
            Arch = arch;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public string GetDotNetRuntimeIdentifier()
        {
            // https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
            return $"{DotNetOsMoniker[Os]}-{DotNetArchMoniker[Arch]}";
        }

        public string GetSlug()
        {
            return GetDotNetRuntimeIdentifier();
        }

        public override string ToString()
        {
            return GetDotNetRuntimeIdentifier();
        }
    }
}
