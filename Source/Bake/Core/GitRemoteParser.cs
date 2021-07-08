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
using System.Text.RegularExpressions;

namespace Bake.Core
{
    public static class GitRemoteParser
    {
        private static readonly Regex Parser = new Regex(
            "^(?<scheme>(http|https)://){0,1}[^@]*?(?<hostname>[^:/]+)(:|/)(?<path>.*)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool TryParse(string str, out Uri url)
        {
            var match = Parser.Match(str);
            if (!match.Success)
            {
                url = null;
                return false;
            }

            url = new UriBuilder
                {
                    Scheme = match.Groups["scheme"].Success
                        ? match.Groups["scheme"].Value
                        : "git",
                    Host = match.Groups["hostname"].Value,
                    Path = match.Groups["path"].Value,
                }.Uri;
            return true;
        }
    }
}