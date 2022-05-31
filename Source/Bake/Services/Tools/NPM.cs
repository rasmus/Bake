using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services.Tools.NPMArguments;

namespace Bake.Services.Tools
{
    public class NPM : INPM
    {
        private readonly IRunnerFactory _runnerFactory;

        public NPM(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public async Task<IToolResult> CIAsync(
            NPMCIArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
            {
                "ci",
                "--only=production"
            };

            var runner = _runnerFactory.CreateRunner(
                "npm.cmd",
                argument.WorkingDirectory,
                arguments.ToArray());

            var runnerResult = await runner.ExecuteAsync(cancellationToken);

            return new ToolResult(runnerResult);
        }
    }
}
