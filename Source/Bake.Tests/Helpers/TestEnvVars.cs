using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;

namespace Bake.Tests.Helpers
{
    public class TestEnvVars : IEnvVars
    {
        private readonly IReadOnlyDictionary<string, string> _environmentVariables;

        public TestEnvVars(
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            _environmentVariables = environmentVariables;
        }

        public Task<IReadOnlyDictionary<string, string>> GetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_environmentVariables);
        }
    }
}