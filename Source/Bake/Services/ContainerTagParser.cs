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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bake.Extensions;
using Bake.ValueObjects;

namespace Bake.Services
{
    public class ContainerTagParser : IContainerTagParser
    {
        private static readonly Regex ImageParser = new(
            @"^
                (?<hostAndPort>[a-z\-0-9\.]+(:[0-9]+){0,1}/){0,1}
                (?<path>[a-z\-0-9/]+/){0,1}
                (?<name>[a-z\-0-9]+)
                (:(?<label>[a-z\-0-9\.]+)){0,1}
            $",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public void Validate(IEnumerable<string> images)
        {
            var invalid = images
                .Where(i => !TryParse(i, out _))
                .OrderBy(i => i)
                .ToArray();

            if (invalid.Any())
            {
                throw new Exception($"These images are invalid: {string.Join(", ", invalid)}");
            }
        }

        public bool TryParse(string tag,
            out ContainerTag containerTag)
        {
            containerTag = null;
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            var match = ImageParser.Match(tag);
            if (!match.Success)
            {
                return false;
            }

            containerTag = new ContainerTag(
                match.GetIfThere("hostAndPort"),
                match.GetIfThere("path"),
                match.GetIfThere("name"),
                match.GetIfThere("label"));
            return true;
        }
    }
}
