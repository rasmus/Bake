using Bake.ValueObjects.Destinations;

namespace Bake.Services
{
    public interface IDestinationParser
    {
        bool TryParse(string str, out Destination destination);
    }
}
