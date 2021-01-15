using System.Threading;
using System.Threading.Tasks;

namespace Bake.Cooking
{
    public interface IKitchen
    {
        Task CookAsync(
            IContext context,
            Book book,
            CancellationToken cancellationToken);
    }
}
