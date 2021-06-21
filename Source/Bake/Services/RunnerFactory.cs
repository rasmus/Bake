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

using System.Collections.Generic;
using System.IO;
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
            string workingDirectory,
            params string[] arguments)
        {
            return new Runner(
                _logger,
                command,
                workingDirectory,
                arguments,
                Empty,
                _fileSystem);
        }

        public IRunner CreateRunner(
                string command,
                string workingDirectory,
                IReadOnlyDictionary<string, string> environmentVariables,
                params string[] arguments)
        {
            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Directory.GetCurrentDirectory();
            }

            return new Runner(
                _logger,
                command,
                workingDirectory,
                arguments,
                environmentVariables,
                _fileSystem);
        }
    }
}
