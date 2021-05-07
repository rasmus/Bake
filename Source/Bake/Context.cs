using Bake.ValueObjects;

namespace Bake
{
    public class Context : IContext
    {
        public Ingredients Ingredients { get; }

        public Context(
            Ingredients ingredients)
        {
            Ingredients = ingredients;
        }
    }
}
