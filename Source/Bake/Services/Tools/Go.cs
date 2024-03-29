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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Services.Tools.GoArguments;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace Bake.Services.Tools
{
    public class Go : IGo
    {
        private static readonly IReadOnlyDictionary<ExecutableOperatingSystem, string> OsMap = new ConcurrentDictionary<ExecutableOperatingSystem, string>
            {
                [ExecutableOperatingSystem.Linux] = "linux",
                [ExecutableOperatingSystem.Windows] = "windows",
                [ExecutableOperatingSystem.MacOSX] = "darwin",
            };
        private static readonly IReadOnlyDictionary<ExecutableArchitecture, string> ArchMap = new ConcurrentDictionary<ExecutableArchitecture, string>
            {
                [ExecutableArchitecture.Intel32] = "386",
                [ExecutableArchitecture.Intel64] = "amd64",
                [ExecutableArchitecture.Arm32] = "arm",
                [ExecutableArchitecture.Arm64] = "arm64"
            };

        private readonly ILogger<Go> _logger;
        private readonly IRunnerFactory _runnerFactory;
        private readonly IDefaults _defaults;

        public Go(
            ILogger<Go> logger,
            IRunnerFactory runnerFactory,
            IDefaults defaults)
        {
            _logger = logger;
            _runnerFactory = runnerFactory;
            _defaults = defaults;
        }

        public async Task<IToolResult> BuildAsync(
            GoBuildArgument argument,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Building Go application {Output}",
                argument.Output);

            var arguments = new[]
                {
                    "build",
                    "-ldflags", _defaults.GoLdFlags,
                    "-o", argument.Output
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "go",
                argument.WorkingDirectory,
                SetupEnvironment(argument.Platform.Os, argument.Platform.Arch),
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> TestAsync(
            GoTestArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new[]
                {
                    "test",
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "go",
                argument.WorkingDirectory,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        private IReadOnlyDictionary<string, string> SetupEnvironment(
            ExecutableOperatingSystem os,
            ExecutableArchitecture arch)
        {
            return new Dictionary<string, string>
                {
                    ["GOOS"] = OsMap[os],
                    ["GOARCH"] = ArchMap[arch],
                    ["CGO_ENABLED"] = "0",
                    ["GOPRIVATE"] = _defaults.GoEnvPrivate,
                };
        }
    }
}
