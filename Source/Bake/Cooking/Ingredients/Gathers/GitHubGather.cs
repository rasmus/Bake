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
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class GitHubGather : IGather
    {
        private static readonly Regex GitHubUrlExtractor = new Regex(
            @"^/(?<owner>[^/]+)/(?<repo>.+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IDefaults _defaults;

        public GitHubGather(
            IDefaults defaults)
        {
            _defaults = defaults;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            GitInformation gitInformation;
            try
            {
                gitInformation = await ingredients.GitTask;
            }
            catch (OperationCanceledException)
            {
                ingredients.FailGitHub();
                return;
            }

            if (!string.Equals(gitInformation.OriginUrl.Host, _defaults.GitHubUrl.Host, StringComparison.OrdinalIgnoreCase))
            {
                ingredients.FailGitHub();
                return;
            }

            var match = GitHubUrlExtractor.Match(gitInformation.OriginUrl.LocalPath);
            if (!match.Success)
            {
                ingredients.FailGitHub();
                return;
            }

            var repo = match.Groups["repo"].Value;
            var lastIndex = repo.LastIndexOf(".git");
            if (lastIndex > 0)
            {
                repo = repo[..lastIndex];
            }

            var url = new Uri($"{_defaults.GitHubUrl.Scheme}://{_defaults.GitHubUrl.Host}/{match.Groups["owner"].Value}/{repo}");
            ingredients.GitHub = new GitHubInformation(
                match.Groups["owner"].Value,
                repo,
                url);
        }
    }
}
