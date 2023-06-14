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
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class DescriptionLimiter : IDescriptionLimiter
    {
        private readonly ILogger<DescriptionLimiter> _logger;

        public DescriptionLimiter(
            ILogger<DescriptionLimiter> logger)
        {
            _logger = logger;
        }

        public string Limit(string markdown, int maxLength)
        {
            if (string.IsNullOrEmpty(markdown) ||
                markdown.Length <= maxLength)
            {
                return markdown ?? string.Empty;
            }

            var lines = markdown
                .Split('\n', '\r');
            var totalHeadlines = lines.Count(l => l.StartsWith("#"));

            for (var i = totalHeadlines; 0 <= i; i--)
            {
                var text = string.Join(Environment.NewLine, TakeHeadlines(lines, i));
                if (string.IsNullOrWhiteSpace(text))
                {
                    break;
                }
                if (text.Length <= maxLength)
                {
                    _logger.LogWarning(
                        "Description text with its length of {CurrentLength} is too long compared to the maximum {MaximumLength}, cutting it at closest header to {NewLength}",
                        markdown.Length,
                        maxLength,
                        text.Length);
                    return text;
                }
            }

            _logger.LogWarning(
                "Description text with its length of {CurrentLength} is too long compared to the maximum {MaximumLength}, but cannot find a good markdown header to cut at so cutting it in the middle of text at {NewLength}",
                markdown.Length,
                maxLength,
                maxLength);

            return markdown[..maxLength];
        }

        private static IEnumerable<string> TakeHeadlines(IEnumerable<string> lines, int headlines)
        {
            var currentHeadline = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    currentHeadline++;
                }
                if (headlines < currentHeadline)
                {
                    yield break;
                }

                yield return line;
            }
        }
    }
}
