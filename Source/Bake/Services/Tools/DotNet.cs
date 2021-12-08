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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using Bake.Services.Tools.DotNetArguments;
using Bake.ValueObjects;

// ReSharper disable StringLiteralTypo

namespace Bake.Services.Tools
{
    public class DotNet : IDotNet
    {
        private static readonly IReadOnlyDictionary<string, string> DotNetEnvironmentVariable = new ConcurrentDictionary<string, string>
            {
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "true",
                ["DOTNET_NOLOGO"] = "true"
            };
        private readonly IRunnerFactory _runnerFactory;
        private readonly ICredentials _credentials;

        public DotNet(
            IRunnerFactory runnerFactory,
            ICredentials credentials)
        {
            _runnerFactory = runnerFactory;
            _credentials = credentials;
        }

        public async Task<IToolResult> ClearNuGetLocalsAsync(
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "nuget",
                    "locals",
                    "http-cache",
                    "--clear"
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                Directory.GetCurrentDirectory(),
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> CleanAsync(
            DotNetCleanArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "clean",
                    argument.FilePath,
                    "--nologo",
                    "--configuration", argument.Configuration,
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> RestoreAsync(
            DotNetRestoreArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "restore",
                    argument.FilePath,
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> BuildAsync(
            DotNetBuildArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                     "build",
                     argument.FilePath,
                     "--nologo",
                     "--configuration", argument.Configuration,
                     $"-p:Version={argument.Version}",
                     $"-p:AssemblyVersion={argument.Version.Major}.0.0.0",
                     $"-p:FileVersion={argument.Version}"
                };

            foreach (var (property, value) in argument.Properties)
            {
                arguments.Add($"-p:{property}={value.ToMsBuildEscaped()}");
            }

            AddIf(!argument.Incremental, arguments, "--no-incremental");
            AddIf(!argument.Restore, arguments, "--no-restore");

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> TestAsync(
            DotNetTestArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "test",
                    argument.FilePath,
                    "--nologo",
                    "--configuration", argument.Configuration,
                };

            AddIf(!argument.Restore, arguments, "--no-restore");
            AddIf(!argument.Build, arguments, "--no-build");

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> PackAsync(
            DotNetPackArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "pack",
                    argument.FilePath,
                    "--nologo",
                    "--configuration", argument.Configuration,
                    $"-p:PackageVersion={argument.Version}"
                };

            AddIf(!argument.Restore, arguments, "--no-restore");
            AddIf(!argument.Build, arguments, "--no-build");
            AddIf(argument.IncludeSource, arguments, "--include-source");
            AddIf(argument.IncludeSymbols, arguments, "--include-symbols");
            AddIf(argument.Build, arguments, "--no-build");
            AddIf(!string.IsNullOrEmpty(argument.Version.Meta), arguments, "--version-suffix", argument.Version.Meta);

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> NuGetPushAsync(
            DotNetNuGetPushArgument argument,
            CancellationToken cancellationToken)
        {
            var apiKey = await _credentials.TryGetNuGetApiKeyAsync(
                argument.Source,
                cancellationToken);

            var arguments = new List<string>
                {
                    "nuget", "push",
                    argument.FilePath,
                    "--api-key", apiKey,
                    "--source", argument.Source.AbsoluteUri,
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> PublishAsync(
            DotNetPublishArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "publish",
                    "--configuration", argument.Configuration,
                    "--nologo",
                    "--output", argument.Output
                };

            if (argument.Platform.Os != ExecutableOperatingSystem.Any)
            {
                arguments.AddRange(new []{"--runtime", argument.Platform.GetDotNetRuntimeIdentifier()});
            }

            AddIf(!argument.Build, arguments, "--no-build");
            AddIf(argument.PublishSingleFile, arguments, "-p:PublishSingleFile=true");
            AddIf(argument.SelfContained, arguments, "--self-contained", "true");
            
            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                DotNetEnvironmentVariable,
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        private static void AddIf(bool predicate, List<string> arguments, params string[] args)
        {
            if (!predicate)
            {
                return;
            }

            arguments.AddRange(args);
        }
    }
}
