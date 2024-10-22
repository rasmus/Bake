// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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
using Bake.ValueObjects;
using Bake.ValueObjects.Credentials;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace Bake.Core
{
    public class Credentials : ICredentials
    {
        private static readonly Regex HostnameInvalidCharacters = new(
            "[^a-z0-9]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly IReadOnlyCollection<string> GitHubTokenPossibilities = new[]
            {
                "github_personal_token",
                "github_token",
            };

        private readonly ILogger<Credentials> _logger;
        private readonly IDefaults _defaults;
        private readonly IEnvironmentVariables _environmentVariables;

        public Credentials(
            ILogger<Credentials> logger,
            IDefaults defaults,
            IEnvironmentVariables environmentVariables)
        {
            _logger = logger;
            _defaults = defaults;
            _environmentVariables = environmentVariables;
        }

        public async Task<string?> TryGetNuGetApiKeyAsync(
            Uri url,
            CancellationToken cancellationToken)
        {
            var hostname = HostnameInvalidCharacters.Replace(url.Host, "_");
            var possibilities = new List<string>
                {
                     "bake_credentials_nuget_apikey",
                    $"bake_credentials_nuget_{hostname}_apikey"
                };

            var gitHubNuGetRegistry = new Uri(_defaults.GitHubNuGetRegistry, UriKind.Absolute);
            if (string.Equals(url.Host, gitHubNuGetRegistry.Host, StringComparison.OrdinalIgnoreCase))
            {
                possibilities.AddRange(GitHubTokenPossibilities);
            }

            var nuGetRegistry = new Uri(_defaults.NuGetRegistry, UriKind.Absolute);
            if (string.Equals(url.Host, nuGetRegistry.Host, StringComparison.OrdinalIgnoreCase))
            {
                possibilities.Add("nuget_apikey");
            }

            var environmentVariables = await _environmentVariables.GetAsync(cancellationToken);

            var value = possibilities
                .Select(k => environmentVariables.TryGetValue(k, out var v) ? v : string.Empty)
                .SkipWhile(string.IsNullOrEmpty)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogInformation(
                    "Dit not find any NuGet credentials for {Url} in any of the environment variables {EnvironmentVariables}",
                    url.AbsoluteUri,
                    string.Join(", ", environmentVariables.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)));
                return null;
            }

            return value;
        }

        public async Task<string> TryGetGitHubTokenAsync(
            Uri url,
            CancellationToken cancellationToken)
        {
            var environmentVariables = await _environmentVariables.GetAsync(cancellationToken);

            var value = GitHubTokenPossibilities
                .Select(k => environmentVariables.TryGetValue(k, out var v) ? v : string.Empty)
                .SkipWhile(string.IsNullOrEmpty)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogInformation(
                    "Dit not find any NuGet credentials for {Url} in any of the environment variables {EnvironmentVariables}",
                    url.AbsoluteUri,
                    string.Join(", ", environmentVariables.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)));
            }

            return value!;
        }

        public async Task<string> TryGetOctopusDeployApiKeyAsync(
            Uri url,
            CancellationToken cancellationToken)
        {
            var hostname = HostnameInvalidCharacters.Replace(url.Host, "_");
            var possibilities = new List<string>
                {
                    "octopus_deploy_apikey",
                    "bake_credentials_octopusdeploy_apikey",
                    $"bake_credentials_octopusdeploy_{hostname}_apikey"
                };

            var environmentVariables = await _environmentVariables.GetAsync(cancellationToken);

            var value = possibilities
                .Select(k => environmentVariables.TryGetValue(k, out var v) ? v : string.Empty)
                .SkipWhile(string.IsNullOrEmpty)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogInformation(
                    "Dit not find any Octopus Deploy API key for {Url} in any of the environment variables {EnvironmentVariables}",
                    url.AbsoluteUri,
                    string.Join(", ", environmentVariables.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)));
            }

            return value!;
        }

        public async Task<DockerLogin?> TryGetDockerLoginAsync(
            ContainerTag containerTag,
            CancellationToken cancellationToken)
        {
            var environmentVariables = await _environmentVariables.GetAsync(cancellationToken);

            if (string.Equals(containerTag.HostAndPort, "ghcr.io", StringComparison.OrdinalIgnoreCase) &&
                environmentVariables.TryGetValue("github_token", out var githubToken))
            {
                return new DockerLogin(
                    containerTag.HostAndPort,
                    githubToken,
                    "ghcr.io");
            }

            var possibilities = new List<(string, string)>();

            if (string.IsNullOrEmpty(containerTag.HostAndPort) || string.Equals(containerTag.HostAndPort, "docker.io", StringComparison.OrdinalIgnoreCase))
            {
                possibilities.Add(("dockerhub", string.Empty));
            }
            else
            {
                var hostname = containerTag.HostAndPort.Split(':', StringSplitOptions.RemoveEmptyEntries)[0];
                var cleanedHostname = HostnameInvalidCharacters.Replace(hostname, "_");
                possibilities.Add(($"bake_credentials_docker_{cleanedHostname}", containerTag.HostAndPort));
            }

            DockerLogin? Get((string, string) t)
            {
                var (e, s) = t;
                var username = environmentVariables.TryGetValue($"{e}_username", out var u) ? u : string.Empty;
                var password = environmentVariables.TryGetValue($"{e}_password", out var p) ? p : string.Empty;

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    _logger.LogInformation($"Did not find any Docker credentials at '{e}_(username/password)'");
                }
                else
                {
                    _logger.LogInformation($"Found Docker credentials at '{e}_(username/password)'");
                }

                return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)
                    ? new DockerLogin(username, password, s)
                    : null;
            }

            var dockerLogin = possibilities
                .Select(Get)
                .FirstOrDefault(l => l != null);

            return dockerLogin;
        }
    }
}
