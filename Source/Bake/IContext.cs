using Bake.ValueObjects;

namespace Bake
{
    public interface IContext
    {
        Ingredients Ingredients { get; }
    }
}
