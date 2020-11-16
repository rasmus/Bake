using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Recipes;

namespace Bake.Books.Composers
{
    public class DotNetComposer : IComposer
    {
        public Task<Recipe> ComposeAsync(string workingDirectory, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
