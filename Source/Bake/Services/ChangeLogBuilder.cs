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
            @"Bump (?<name>[^\s]+) from (?<from>[0-9\.\-a-z]+) to (?<to>[0-9\.\-a-z]+)( in (?<project>[^\s]+)){0,1}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IReadOnlyDictionary<ChangeType, IReadOnlyCollection<Change>> Build(IReadOnlyCollection<PullRequest> pullRequests)
        {
            var dependencyChanges =
                (
                    from pr in pullRequests
                    let m = DependencyDetector.Match(pr.Title)
                    where m.Success
                    let p = m.Groups["project"]
                    let a = new
                    {
                        fromVersion = SemVer.Parse(m.Groups["from"].Value, true),
                        to = SemVer.Parse(m.Groups["to"].Value, true),
                        dependency = m.Groups["name"].Value,
                        project = p.Success ? p.Value : string.Empty,
                        pr,
                    }
                    group a by new { a.dependency, a.project }
                    into g
                    orderby g.Key.project, g.Key.dependency
                    select new DependencyChange(
                        g.Key.dependency,
                        g.Key.project,
                        g.Select(x => x.pr.Number).OrderBy(i => i).ToArray(),
                        g.Min(x => x.fromVersion),
                        g.Max(x => x.to))
                )
                .OrderBy(c => c.Name)
                .ToArray();

            var dependencyPRs = new HashSet<int>(dependencyChanges.SelectMany(d => d.PullRequests));

            var otherChanges = pullRequests
                .Where(pr => !dependencyPRs.Contains(pr.Number))
                .OrderBy(pr => pr.Number)
                .Select(pr => pr.ToChange())
                .ToArray();

            return new Dictionary<ChangeType, IReadOnlyCollection<Change>>
                {
                    [ChangeType.Other] = otherChanges,
                    [ChangeType.Dependency] = dependencyChanges.Select(d => d.ToChange()).ToArray(),
                };
        }

        private class DependencyChange
        {
            public string Name { get; }
            public string Project { get; }
            public int[] PullRequests { get; }
            public SemVer From { get; }
            public SemVer To { get; }

            public DependencyChange(
                string name,
                string project,
                int[] pullRequests,
                SemVer from,
                SemVer to)
            {
                Name = name;
                Project = project;
                PullRequests = pullRequests;
                From = from;
                To = to;
            }

            public Change ToChange()
            {
                var message = string.IsNullOrEmpty(Project)
                    ? $"Bump {Name} {From} to {To} ({string.Join(", ", PullRequests.Select(pr => $"#{pr}"))})"
                    : $"Bump {Name} {From} to {To} in {Project} ({string.Join(", ", PullRequests.Select(pr => $"#{pr}"))})";

                return new Change(ChangeType.Dependency, message);
            }
        }
    }
}
