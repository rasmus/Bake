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

namespace Bake.ValueObjects.Destinations
{
    public abstract class Destination
    {
        private const char Separator = '>';

        public static bool TryParse(string str, out Destination destination)
        {
            destination = null;

            var parts = str.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0])
            {
                case DestinationNames.NuGet:
                    if (parts.Length == 1)
                    {
                        destination = new NuGetRegistryDestination(
                            new Uri("https://api.nuget.org/v3/index.json", UriKind.Absolute));
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
