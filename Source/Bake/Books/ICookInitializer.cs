using Bake.Books.Cooks;

namespace Bake.Books
{
    public interface ICookInitializer
    {
        ICookInitializer DependOn<T>()
            where T : ICook;
    }
}
