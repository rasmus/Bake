using System;
using Bake.Core;
using Bake.ValueObjects.Destinations;

namespace Bake.Services
{
    public class DestinationParser : IDestinationParser
    {
        private const char Separator = '>';

        private readonly IDefaults _defaults;

        public DestinationParser(
            IDefaults defaults)
        {
            _defaults = defaults;
        }

        public bool TryParse(string str, out Destination destination)
        {
            destination = null;

            var parts = str.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case Names.ArtifactTypes.NuGet:
                    if (parts.Length == 1)
                    {
                        destination = new NuGetRegistryDestination(_defaults.NuGetRegistry);
                        return true;
                    }

                    if (string.Equals(parts[1], Names.DynamicDestinations.GitHub))
                    {
                        destination = new DynamicDestination(
                            Names.ArtifactTypes.NuGet,
                            Names.DynamicDestinations.GitHub);
                        return true;
                    }

                    if (!Uri.TryCreate(parts[1], UriKind.Absolute, out var url))
                    {
                        return false;
                    }

                    destination = new NuGetRegistryDestination(url);
                    
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(str));
            }
        }
    }
}
