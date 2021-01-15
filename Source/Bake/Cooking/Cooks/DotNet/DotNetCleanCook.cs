using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Recipes.DotNet;
using Bake.Services;

namespace Bake.Cooking.Cooks.DotNet
{
    public class DotNetCleanCook : Cook<DotNetCleanSolution>
    {
        public override string Name => "dotnet:clean";

        private readonly IRunnerFactory _runnerFactory;

        public DotNetCleanCook(
            IRunnerFactory runnerFactory)
        {
            _runnerFactory = runnerFactory;
        }

        protected override Task CookAsync(
            IContext context,
            DotNetCleanSolution recipe,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
