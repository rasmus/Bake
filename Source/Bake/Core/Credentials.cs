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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Core
{
    public class Credentials : ICredentials
    {
        private static readonly Regex InvalidCharacters = new Regex(
            "[^a-z0-9]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IDefaults _defaults;
        private readonly IEnvironmentVariables _environmentVariables;

        public Credentials(
            IDefaults defaults,
            IEnvironmentVariables environmentVariables)
        {
            _defaults = defaults;
            _environmentVariables = environmentVariables;
        }

        public async Task<string> GetNuGetApiKeyAsync(
            Uri url,
            CancellationToken cancellationToken)
        {
            var hostname = InvalidCharacters.Replace(url.Host, "_");
            var possibilities = new List<string>
                {
                    $"bake_credentials_nuget_{hostname}_apikey"
                };

            if (string.Equals(url.Host, _defaults.GitHubNuGetRegistry.Host, StringComparison.OrdinalIgnoreCase))
            {
                possibilities.Add("github_token");
            }

            var environmentVariables = await _environmentVariables.GetAsync(cancellationToken);

            var value = possibilities
                .Select(k => environmentVariables.TryGetValue(k, out var v) ? v : string.Empty)
                .SkipWhile(string.IsNullOrEmpty)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(
                    $"No environment variable named any of '{string.Join(", ", possibilities)}' (case insensitive) with credentials for {url}");
            }

            return value;
        }
    }
}
