using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Services
{
    public class Command : ICommand
    {
        private readonly string _command;
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

        public Command(
            string command,
            string workingDirectory,
            IReadOnlyCollection<string> arguments)
        {
            _command = command;
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
            Console.Out.WriteLine(output);
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
