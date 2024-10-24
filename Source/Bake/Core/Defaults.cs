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

// ReSharper disable StringLiteralTypo

using System.Globalization;

namespace Bake.Core
{
    public class Defaults : IDefaults
    {
        private readonly IEnvironmentVariables _environmentVariables;

        public string GitHubUrl { get; private set; } = "https://github.com/";
        public string GitHubNuGetRegistry { get; private set; } = "https://nuget.pkg.github.com/OWNER/index.json";
        public string NuGetRegistry { get; private set; } = "https://api.nuget.org/v3/index.json";
        public string DockerHubUserRegistry { get; private set; } = "{USER}/";
        public string GitHubUserRegistry { get; private set; } = "ghcr.io/{USER}/";
        public bool DockerBuildCompress { get; private set; } = true;
        public string GoLdFlags { get; private set; } = "-s -w";
        public string GoEnvPrivate { get; private set; } = "direct";
        public string DotNetRollForward { get; private set; } = "LatestMajor";
        public TimeSpan BakeIngredientsGatherTimeout { get; private set; } = TimeSpan.FromMinutes(5);
        public TimeSpan BakeComposeTimeout { get; private set; } = TimeSpan.FromMinutes(5);

        public Defaults(
            IEnvironmentVariables environmentVariables)
        {
            _environmentVariables = environmentVariables;
        }

        public async Task InitializeAsync(
            CancellationToken cancellationToken)
        {
            var e = await _environmentVariables.GetAsync(cancellationToken);

            GitHubUrl = GetString(e, "github_url", GitHubUrl);
            GitHubNuGetRegistry = GetString(e, "github_packages_nuget_url", GitHubNuGetRegistry);
            GitHubUserRegistry = GetString(e, "github_packages_container_url", GitHubUserRegistry);
            NuGetRegistry = GetString(e, "nuget_url", NuGetRegistry);
            DockerHubUserRegistry = GetString(e, "dockerhub_user_url", DockerHubUserRegistry);
            DockerBuildCompress = GetBool(e, "docker_build_compress", true);
            GoLdFlags = GetString(e, "go_ldflags", GoLdFlags);
            GoEnvPrivate = GetString(e, "go_env_goprivate", GoEnvPrivate);
            DotNetRollForward = GetString(e, "dotnet_roll_forward", DotNetRollForward);
            BakeIngredientsGatherTimeout = TimeSpan.FromSeconds(GetDouble(e, "bake_ingredients_gather_timeout_seconds", BakeIngredientsGatherTimeout.TotalSeconds));
            BakeComposeTimeout = TimeSpan.FromSeconds(GetDouble(e, "bake_compose_timeout_seconds", BakeComposeTimeout.TotalSeconds));
        }

        private static bool GetBool(
            IReadOnlyDictionary<string, string> environmentVariables,
            string name,
            bool defaultValue)
        {
            var value = GetString(environmentVariables, name, defaultValue.ToString());
            if (!bool.TryParse(value, out var b))
            {
                throw new InvalidOperationException(
                    $"Cannot parse value '{value}' to bool for key '{name}'");
            }

            return b;
        }

        private static double GetDouble(
            IReadOnlyDictionary<string, string> environmentVariables,
            string name,
            double defaultValue)
        {
            var value = GetString(environmentVariables, name, defaultValue.ToString(CultureInfo.InvariantCulture));
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                throw new InvalidOperationException(
                    $"Cannot parse value '{value}' to double for key '{name}'");
            }

            return d;
        }

        private static string GetString(
            IReadOnlyDictionary<string, string> environmentVariables,
            string name,
            string defaultValue)
        {
            return environmentVariables.TryGetValue($"bake_defaults_{name}", out var v) && !string.IsNullOrEmpty(v)
                ? v
                : defaultValue;
        }
    }
}
