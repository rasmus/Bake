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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services.Tools.HelmArguments;

namespace Bake.Services.Tools
{
    public class Helm : IHelm
    {
        private readonly IRunnerFactory _runnerFactory;

        public Helm(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }


        public async Task<IToolResult> LintAsync(
            HelmLintArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new[]
                {
                    "lint",
                    argument.ChartDirectory,
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "helm",
                Directory.GetCurrentDirectory(),
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }

        public async Task<IToolResult> PackageAsync(
            HelmPackageArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new[]
                {
                    "package",
                    argument.ChartDirectory,
                    "--dependency-update",
                    "--version", argument.Version.ToString(),
                    "--destination", argument.OutputDirectory
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "helm",
                Directory.GetCurrentDirectory(),
                arguments);

            var runnerResult = await buildRunner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }
    }
}
