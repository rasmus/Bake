// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
                    return false;
            }
        }
    }
}
