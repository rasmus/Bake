﻿// MIT License
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

using System.Text.RegularExpressions;
using Bake.Extensions;
using Bake.ValueObjects;

namespace Bake.Services
{
    public class GoModParser : IGoModParser
    {
        private static readonly Regex ModuleParser = new(
            @"^\s*module\s+([a-z0-9\-\./]+?/){0,1}?(?<name>[a-z0-9\-_\.]+)(/v(?<version>[0-9\.]+)){0,1}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        public bool TryParse(string str, out GoModuleName goModuleName)
        {
            goModuleName = null;
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var match = ModuleParser.Match(str);
            if (!match.Success)
            {
                return false;
            }

            goModuleName = new GoModuleName(
                match.Groups["name"].Value,
                match.GetIfThere("version"));
            return true;
        }
    }
}