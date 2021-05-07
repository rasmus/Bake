using System.Threading;
using System.Threading.Tasks;

namespace Bake.Cooking.Ingredients.Gathers
{
    public interface IGather
    {
        Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken);
    }
}
