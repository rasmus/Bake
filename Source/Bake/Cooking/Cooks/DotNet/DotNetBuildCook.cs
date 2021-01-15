using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Recipes.DotNet;
using Bake.Services;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetBuildCook : Cook<DotNetBuildSolution>
    {
        public override string Name => "dotnet:build";

        private readonly IRunnerFactory _runnerFactory;

        public DotNetBuildCook(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        protected override Task CookAsync(
            IContext context,
            DotNetBuildSolution recipe,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
