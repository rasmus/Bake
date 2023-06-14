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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Bake.Core;
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class RunnerFactory : IRunnerFactory
    {
        private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

        private readonly ILogger<RunnerFactory> _logger;
        private readonly IFileSystem _fileSystem;

        public RunnerFactory(
            ILogger<RunnerFactory> logger,
            IFileSystem fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public IRunner CreateRunner(
                string command,
                string? workingDirectory,
                bool asShell,
                IReadOnlyDictionary<string, string>? environmentVariables,
                params string[] arguments)
        {
            if (asShell)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    arguments = new [] { "/c", command, }
                        .Concat(arguments)
                        .ToArray();
                    
                    return CreateRunner(
                        "cmd",
                        workingDirectory,
                        environmentVariables,
                        arguments);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    arguments = new[] { "-c", $"{command} \"$@\"", "bash" }
                        .Concat(arguments)
                        .ToArray();

                    return CreateRunner(
                        "/bin/bash",
                        workingDirectory,
                        environmentVariables,
                        arguments);
                }

                throw new Exception($"Unable to run as shell for platform {RuntimeInformation.RuntimeIdentifier}");
            }

            return CreateRunner(
                command,
                workingDirectory,
                environmentVariables,
                arguments);
        }

        private IRunner CreateRunner(
            string command,
            string? workingDirectory,
            IReadOnlyDictionary<string, string>? environmentVariables,
            params string[] arguments)
        {
            return new Runner(
                _logger,
                command,
                string.IsNullOrEmpty(workingDirectory)
                    ? Directory.GetCurrentDirectory()
                    : workingDirectory,
                arguments,
                environmentVariables ?? Empty,
                _fileSystem);
        }
    }
}
