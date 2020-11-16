using System;
using System.Threading;
using System.Threading.Tasks;
using Bake.Books.Recipes;

namespace Bake.Books.Cooks
{
    public interface ICook
    {
        Type CanCook { get; }

        void Initialize(ICookInitializer cookInitializer);

        Task CookAsync(
            IContext context,
            Recipe recipe,
            CancellationToken cancellationToken);
    }
}
