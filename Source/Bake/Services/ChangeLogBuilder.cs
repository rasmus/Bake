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
using Bake.Core;
using Bake.ValueObjects;

namespace Bake.Services
{
    public class ChangeLogBuilder : IChangeLogBuilder
    {
        private static readonly Regex DependencyDetector = new(
            @"Bump (?<name>[^s]+) from (?<from>[0-9\.\-a-z]+) to (?<to>[0-9\.\-a-z]+)( (?<project>[a-z\-\.0-9])){0,1}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IReadOnlyCollection<Change> Build(IReadOnlyCollection<PullRequest> pullRequests)
        {
            var dependencies = pullRequests
                .Select(pr => new
                {
                    match = DependencyDetector.Match(pr.Title),
                    pullRequest = pr,
                })
                .Where(a => a.match.Success)
                .Select(a => new
                {
                    from = SemVer.Parse(a.match.Groups["from"].Value),
                    to = SemVer.Parse(a.match.Groups["to"].Value),
                    dependency = a.match.Groups["name"].Value,
                    project = a.match.Groups["project"].Value,
                    pullRequestNumber = a.pullRequest.Number,
                })
                .GroupBy(a => new { a.dependency, a.project })
                .Select(g => new
                {
                    from = g.Min(a => a.from),
                    to = g.Max(a => a.to),
                    g.Key.dependency,
                    g.Key.project,
                    pullRequestNumbers = g.Select(a => a.pullRequestNumber).ToArray(),
                })
                .OrderBy(a => a.project)
                .ThenBy(a => a.dependency);

            return ArraySegment<Change>.Empty;
        }
    }
}
