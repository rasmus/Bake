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
using System.Linq;
using System.Text.RegularExpressions;
using Bake.Core;
using Bake.ValueObjects.DotNet;

namespace Bake.Services
{
    public class DotNetTfmParser : IDotNetTfmParser
    {
        // https://docs.microsoft.com/en-us/dotnet/standard/frameworks
        // https://docs.microsoft.com/en-us/dotnet/standard/frameworks#net-5-os-specific-tfms

        private static readonly Regex MonikerParser = new(
            @"^net(?<framework>standard|coreapp|)(?<version>[0-9\.]+)(-(?<platform>[a-z]+)){0,1}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool TryParse(string moniker, out TargetFrameworkVersion targetFrameworkVersion)
        {
            targetFrameworkVersion = null;
            if (string.IsNullOrEmpty(moniker))
            {
                return false;
            }

            moniker = moniker.Trim();
            var match = MonikerParser.Match(moniker);
            if (!match.Success)
            {
                return false;
            }

            var version = match.Groups["version"].Value;
            var framework = match.Groups["framework"].Value;

            if (int.TryParse(version, out _))
            {
                if (!string.IsNullOrEmpty(framework))
                {
                    return false;
                }

                targetFrameworkVersion = new TargetFrameworkVersion(
                    moniker,
                    SemVer.With(
                        GetPositionOrZero(version, 0),
                        GetPositionOrZero(version, 1),
                        GetPositionOrZero(version, 2)),
                    TargetFramework.NetFramework);
                return true;
            }

            if (!double.TryParse(version, out  _))
            {
                return false;
            }

            var versionParts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (versionParts.Length != 2 && versionParts.All(p => p.Length == 1))
            {
                return false;
            }

            targetFrameworkVersion = new TargetFrameworkVersion(
                moniker,
                SemVer.With(
                    int.Parse(versionParts[0]),
                    int.Parse(versionParts[1])),
                string.Equals(match.Groups["framework"].Value, "standard", StringComparison.OrdinalIgnoreCase)
                    ? TargetFramework.NetStandard
                    : TargetFramework.Net);
            return true;
        }

        private static int GetPositionOrZero(string str, int i)
        {
            return i < str.Length
                ? int.Parse(new string(new[]{str[i]}))
                : 0;
        }
    }
}
