using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects;

namespace Bake.Cooking
{
    public interface IKitchen
    {
        Task<bool> CookAsync(IContext context,
            Book book,
            CancellationToken cancellationToken);
    }
}
