using System.Threading;
using System.Threading.Tasks;

namespace Bake.Cookbooks.Recipes
{
    public interface IRecipe
    {
        string Name { get; }

        Task BakeAsync(string workingDirectory, CancellationToken cancellationToken);
    }
}
