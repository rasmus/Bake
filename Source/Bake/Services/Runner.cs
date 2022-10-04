// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class Runner : IRunner
    {
        private readonly ILogger _logger;
        private readonly string _command;
        private readonly string _workingDirectory;
        private readonly IReadOnlyCollection<string> _arguments;
        private readonly Process _process;
        private readonly Subject<string> _stdOut = new();
        private readonly Subject<string> _stdErr = new();
        private readonly IFile _log;

        public IObservable<string> StdOut => _stdOut;
        public IObservable<string> StdErr => _stdErr;

        public Runner(
            ILogger logger,
            string command,
            string workingDirectory,
            IReadOnlyCollection<string> arguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            IFileSystem fileSystem)
        {
            _logger = logger;
            _command = command;
            _workingDirectory = workingDirectory;
            _arguments = arguments;
            _process = CreateProcess(
                command,
                workingDirectory,
                arguments,
                environmentVariables);
            _process.OutputDataReceived += OnStdOut;
            _process.ErrorDataReceived += OnStdErr;

            _log = fileSystem.OpenTempFile();
        }

        public Task<IRunnerResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => InternalExecuteAsync(cancellationToken), cancellationToken);
        }

        private async Task<IRunnerResult> InternalExecuteAsync(CancellationToken cancellationToken)
        {
            await using var stream = await _log.OpenWriteAsync(cancellationToken);

            Console.WriteLine($"{_command} {string.Join(" ", _arguments)}");
            _logger.LogTrace(
                "Executing {Program} {Arguments} in {Directory}",
                _command,
                string.Join(" ", _arguments),
                _workingDirectory);

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            await _process.WaitForExitAsync(cancellationToken);

            return new RunnerResult(
                _process.ExitCode,
                _log);
        }

        public override string ToString()
        {
            return $"{_command} {string.Join(" ", _arguments)}";
        }

        public ValueTask DisposeAsync()
        {
            _process.OutputDataReceived -= OnStdOut;
            _process.ErrorDataReceived -= OnStdErr;

            _stdErr.Dispose();
            _stdOut.Dispose();

            return new ValueTask();
        }

        
        private void OnStdOut(object sender, DataReceivedEventArgs e)
        {
            var output = CleanupOutput(e.Data);
            if (!string.IsNullOrWhiteSpace(output))
            {
                _logger.LogInformation(output);
            }
            _stdOut.OnNext(output);
        }

        private void OnStdErr(object sender, DataReceivedEventArgs e)
        {
            var output = CleanupOutput(e.Data);
            if (!string.IsNullOrWhiteSpace(output))
            {
                _logger.LogError(output);
            }
            _stdErr.OnNext(output);
        }

        private static string CleanupOutput(string str)
        {
            return (str ?? string.Empty)
                .Trim('\n', '\r');
        }

        private static Process CreateProcess(
            string command,
            string workingDirectory,
            IEnumerable<string> arguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            var argumentsString = string.Join(" ", arguments);
            var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                        {
                            FileName = command,
                            Arguments = argumentsString,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = false,
                            WorkingDirectory = workingDirectory,
                        },
                };

            foreach (var (key, value) in environmentVariables)
            {
                process.StartInfo.EnvironmentVariables[key] = value;
            }

            return process;
        }
    }
}
