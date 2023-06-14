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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bake.Extensions
{
    public static class StringExtensions
    {
        static StringExtensions()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static readonly Regex SlugInvalidCharacters = new Regex(
            @"[^a-z0-9-]+",
            RegexOptions.Compiled);

        private static readonly Regex SlugMultipleDash = new Regex(
            @"\-{2,}",
            RegexOptions.Compiled);

        public static string ToSlug(this string text)
        {
            var str = text.RemoveAccent();
            str = str.ToLowerInvariant();
            str = SlugInvalidCharacters.Replace(str, "-");
            str = str.Trim('-');
            str = SlugMultipleDash.Replace(str, "-");

            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException($"'{text}' cannot be converted to a slug");
            }

            return str;
        }

        public static string RemoveAccent(this string text) 
        {
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(text); 
            return Encoding.ASCII.GetString(bytes); 
        }

        private static readonly IReadOnlyDictionary<char, string> MsBuildEscapeMap = new Dictionary<char, string>
            {
                ['\r'] = "%0D",
                ['\n'] = "%0A",
                [';' ] =  "%3B",
                [',' ] = "%2C",
                [' ' ] = "%20",
                ['"' ] = "\\\"",
            };
        public static string ToMsBuildEscaped(this string str)
        {
            return str.Aggregate(
                    new StringBuilder(),
                    (b, c) => MsBuildEscapeMap.TryGetValue(c, out var replacement)
                        ? b.Append(replacement)
                        : b.Append(c))
                .ToString();
        }
    }
}
