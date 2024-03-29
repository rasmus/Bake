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

using System.Collections.Generic;
using System.Linq;
using Bake.Core;

namespace Bake.Services.Tools
{
    public class ToolResult : IToolResult
    {
        public static IToolResult Successful(IEnumerable<IFile> logs) => new LogOnlyToolResult(true, logs);
        public static IToolResult Unsuccessful(IEnumerable<IFile> logs) => new LogOnlyToolResult(false, logs);

        public bool WasSuccessful => _runnerResult.WasSuccessful;
        public IReadOnlyCollection<IFile> Logs => _runnerResult.Logs;

        private readonly IRunnerResult _runnerResult;

        public ToolResult(
            IRunnerResult runnerResult)
        {
            _runnerResult = runnerResult;
        }

        public void Dispose()
        {
            _runnerResult.Dispose();
        }

        private class LogOnlyToolResult : IToolResult
        {
            public bool WasSuccessful { get; }
            public IReadOnlyCollection<IFile> Logs { get; }

            public LogOnlyToolResult(
                bool wasSuccessful,
                IEnumerable<IFile> logs)
            {
                WasSuccessful = wasSuccessful;
                Logs = logs.ToArray();
            }

            public void Dispose()
            {
                foreach (var log in Logs)
                {
                    log.Dispose();
                }
            }
        }
    }
}
