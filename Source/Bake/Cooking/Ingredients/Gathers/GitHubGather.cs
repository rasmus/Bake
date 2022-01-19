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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class GitHubGather : IGather
    {
        private static readonly Regex GitHubUrlExtractor = new(
            @"^/(?<owner>[^/]+)/(?<repo>.+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Uri GitHubApiUrl = new("https://api.github.com/", UriKind.Absolute);

        private readonly ILogger<GitHubGather> _logger;
        private readonly IDefaults _defaults;

        public GitHubGather(
            ILogger<GitHubGather> logger,
            IDefaults defaults)
        {
            _logger = logger;
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
                _logger.LogInformation("No git information, thus failed to get any GitHub information");
                ingredients.FailGitHub();
                return;
            }

            Uri apiUrl;
            var gitHubUrl = new Uri(_defaults.GitHubUrl);
            if (string.Equals(gitInformation.OriginUrl.Host, gitHubUrl.Host, StringComparison.OrdinalIgnoreCase))
            {
                apiUrl = GitHubApiUrl;
                _logger.LogInformation(
                    "Public GitHub detected on origin {Url}. Setting API URL to {ApiUrl}",
                    gitInformation.OriginUrl,
                    apiUrl);
            }
            else if (string.Equals(
                "github",
                gitInformation.OriginUrl.Host.Split('.', StringSplitOptions.RemoveEmptyEntries).First(),
                StringComparison.OrdinalIgnoreCase))
            {
                apiUrl = new UriBuilder(gitInformation.OriginUrl)
                    {
                        Scheme = "https",
                        Path = "/api/v3"
                    }.Uri;
                _logger.LogInformation(
                    "The first part of the URL {Url} is 'github', thus we expect that its GitHub Enterprise. Setting API URL to {ApiUrl}",
                    gitInformation.OriginUrl,
                    apiUrl);
            }
            else
            {
                _logger.LogWarning(
                    "Could not determine public nor enterprise GitHub URL from {RemoteUrl}",
                    gitInformation.OriginUrl);
                ingredients.FailGitHub();
                return;
            }

            var match = GitHubUrlExtractor.Match(gitInformation.OriginUrl.LocalPath);
            if (!match.Success)
            {
                _logger.LogWarning(
                    "Could not determine GitHub owner and repository from {RemoteUrl}",
                    gitInformation.OriginUrl);
                ingredients.FailGitHub();
                return;
            }

            var repo = match.Groups["repo"].Value;
            var lastIndex = repo.LastIndexOf(".git");
            if (lastIndex > 0)
            {
                repo = repo[..lastIndex];
            }

            var url = new Uri($"{gitHubUrl.Scheme}://{gitHubUrl.Host}/{match.Groups["owner"].Value}/{repo}");
            ingredients.GitHub = new GitHubInformation(
                match.Groups["owner"].Value,
                repo,
                url,
                apiUrl);
        }
    }
}
