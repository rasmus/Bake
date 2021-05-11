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
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly Subject<string> _stdOut = new Subject<string>();
        private readonly Subject<string> _stdErr = new Subject<string>();
        private readonly List<string> _out = new List<string>();
        private readonly List<string> _err = new List<string>();

        public IObservable<string> StdOut => _stdOut;
        public IObservable<string> StdErr => _stdErr;
        public IReadOnlyCollection<string> Out => _out;
        public IReadOnlyCollection<string> Err => _err;

        public Runner(
            ILogger logger,
            string command,
            string workingDirectory,
            IReadOnlyCollection<string> arguments)
        {
            _logger = logger;
            _command = command;
            _workingDirectory = workingDirectory;
            _arguments = arguments;
            _process = CreateProcess(
                command,
                workingDirectory,
                arguments);
            _process.OutputDataReceived += OnStdOut;
            _process.ErrorDataReceived += OnStdErr;
        }

        public Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(Execute, cancellationToken);
        }

        public override string ToString()
        {
            return $"{_command} {string.Join(" ", _arguments)}";
        }

        private int Execute()
        {
            _logger.LogDebug(
                "Executing '{Program} {Arguments}' in {Directory}",
                _command,
                string.Join(" ", _arguments),
                _workingDirectory);

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            _process.WaitForExit();
            return _process.ExitCode;
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
            _out.Add(output);
            _stdOut.OnNext(output);
            Console.WriteLine(output);
        }

        private void OnStdErr(object sender, DataReceivedEventArgs e)
        {
            var output = CleanupOutput(e.Data);
            _err.Add(output);
            _stdErr.OnNext(output);
            Console.Error.WriteLine(output);
        }

        private static string CleanupOutput(string str)
        {
            return (str ?? string.Empty)
                .Trim('\n', '\r');
        }

        private static Process CreateProcess(
            string command,
            string workingDirectory,
            IEnumerable<string> arguments)
        {
            var argumentsString = string.Join(" ", arguments);
            return new Process
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
        }
    }
}
