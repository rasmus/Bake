﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.Extensions;
using Bake.Services.DotNetArgumentBuilders;

namespace Bake.Services
{
    public class DotNet : IDotNet
    {
        private readonly IRunnerFactory _runnerFactory;

        public DotNet(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        public async Task CleanAsync(
            DotNetCleanArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "clean"
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);
        }

        public async Task BuildAsync(
            DotNetBuildArgument argument,
            CancellationToken cancellationToken)
        {
            var arguments = new List<string>
                {
                    "build"
                };

            var buildRunner = _runnerFactory.CreateRunner(
                "dotnet",
                argument.WorkingDirectory,
                arguments);

            var result = await buildRunner.ExecuteAsync(cancellationToken);
        }
    }
}
