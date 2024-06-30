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

using Bake.Extensions;
using Bake.Services.Tools.DockerArguments;

namespace Bake.Services.Tools
{
    public class Docker : IDocker
    {
        private static readonly IReadOnlyDictionary<string, string> EnvironmentVariables = new Dictionary<string, string>
        {
            ["DOCKER_BUILDKIT"] = "1",
        };

        private readonly IRunnerFactory _runnerFactory;

        public Docker(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public async Task<IToolResult> BuildAsync(
            DockerBuildArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "build",
                    "--pull",
                    "--no-cache",
                    "--progress", "plain",
                };
            arguments.AddRange(argument.Tags.SelectMany(t => new []{"-t", t}));
            arguments.Add(".");
            if (argument.Compress)
            {
                arguments.Add("--compress");
            }

            foreach (var secretMount in argument.SecretMounts)
            {
                arguments.AddRange(new[]{"--secret", $"id={secretMount.Key},src={secretMount.Value}"});   
            }

            foreach (var label in argument.Labels)
            {
                arguments.AddRange(new[]{"--label", $"\"{label.Key}={label.Value}\""});
            }

            var runner = _runnerFactory.CreateRunner(
                "docker",
                argument.WorkingDirectory,
                EnvironmentVariables,
                arguments.ToArray());

            var result = await runner.ExecuteAsync(cancellationToken);

            return new ToolResult(result);
        }

        public async Task<IToolResult> PushAsync(
            DockerPushArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new[]
                {
                    "push",
                    argument.Tag.ToString(),
                };

            var runner = _runnerFactory.CreateRunner(
                "docker",
                Directory.GetCurrentDirectory(),
                EnvironmentVariables,
                arguments);

            var result = await runner.ExecuteAsync(cancellationToken);

            return new ToolResult(result);
        }

        public async Task<IToolResult> LoginAsync(
            DockerLoginArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new[]
                {
                    "login",
                    "--username", argument.Login.Username,
                    "--password", argument.Login.Password, // TODO: Pass via STDIN
                    argument.Login.Server
                };

            var runner = _runnerFactory.CreateRunner(
                "docker",
                Directory.GetCurrentDirectory(),
                arguments);

            var result = await runner.ExecuteAsync(cancellationToken);

            return new ToolResult(result);
        }
    }
}
