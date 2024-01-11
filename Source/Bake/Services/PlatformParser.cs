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

using System.Collections.Concurrent;
using Bake.ValueObjects;

// ReSharper disable StringLiteralTypo

namespace Bake.Services
{
    public class PlatformParser : IPlatformParser
    {
        private static readonly IReadOnlyDictionary<string, ExecutableArchitecture> ArchNames = new ConcurrentDictionary<string, ExecutableArchitecture>(StringComparer.OrdinalIgnoreCase)
        {
                ["x86"] = ExecutableArchitecture.Intel32,
                ["x64"] = ExecutableArchitecture.Intel64,
                ["x86_64"] = ExecutableArchitecture.Intel64,
                ["arm"] = ExecutableArchitecture.Arm32,
                ["arm32"] = ExecutableArchitecture.Arm32,
                ["arm64"] = ExecutableArchitecture.Arm64,
            };
        private static readonly IReadOnlyDictionary<string, ExecutableOperatingSystem> OsNames = new ConcurrentDictionary<string, ExecutableOperatingSystem>(StringComparer.OrdinalIgnoreCase)
            {
                ["win"] = ExecutableOperatingSystem.Windows,
                ["windows"] = ExecutableOperatingSystem.Windows,
                ["linux"] = ExecutableOperatingSystem.Linux,
                ["mac"] = ExecutableOperatingSystem.MacOSX,
                ["macos"] = ExecutableOperatingSystem.MacOSX,
                ["macosx"] = ExecutableOperatingSystem.MacOSX,
            };

        public bool TryParse(string str, out Platform? platform)
        {
            platform = null;

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var parts = str
                .Trim()
                .Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            if (!OsNames.TryGetValue(parts[0].Trim(), out var os))
            {
                return false;
            }

            if (!ArchNames.TryGetValue(parts[1].Trim(), out var arch))
            {
                return false;
            }

            platform = new Platform(os, arch);
            return true;
        }
    }
}
