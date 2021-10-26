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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bake.ValueObjects;

namespace Bake.Core
{
    public class NuGetConfiguration : INuGetConfiguration
    {
        private readonly ICredentials _credentials;

        public NuGetConfiguration(
            ICredentials credentials)
        {
            _credentials = credentials;
        }

        public async Task<string> GenerateAsync(
            IReadOnlyCollection<NuGetSource> nuGetSources,
            CancellationToken cancellationToken)
        {
            var packageSourceCredentials = await GeneratePackageSourceCredentialsAsync(
                nuGetSources,
                cancellationToken)
                .ToListAsync(cancellationToken);

            var xDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("configuration",
                    new XElement("packageSources", GeneratePackageSources(nuGetSources)),
                    new XElement("packageSourceCredentials", packageSourceCredentials)));

            return xDocument.ToString(SaveOptions.None);
        }

        private static IEnumerable<XElement> GeneratePackageSources(IEnumerable<NuGetSource> nuGetSources)
        {
            yield return new XElement("clear");

            foreach (var nuGetSource in nuGetSources)
            {
                yield return new XElement(
                    "add",
                    new XAttribute("key", nuGetSource.Key),
                    new XAttribute("value", nuGetSource.Path));
            }
        }

        private async IAsyncEnumerable<XElement> GeneratePackageSourceCredentialsAsync(
            IEnumerable<NuGetSource> nuGetSources,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var nuGetSource in nuGetSources)
            {
                var xElements = new List<XElement>
                    {
                        new XElement("add", new XAttribute("key", "Username"), new XAttribute("value", "USERNAME")),
                    };

                var apiKey = Uri.TryCreate(nuGetSource.Path, UriKind.Absolute, out var url)
                    ? await _credentials.TryGetNuGetApiKeyAsync(url, cancellationToken)
                    : string.Empty;

                if (!string.IsNullOrEmpty(apiKey))
                {
                    xElements.Add(new XElement("add", new XAttribute("key", "ClearTextPassword"), new XAttribute("value", apiKey)));
                }

                yield return new XElement(
                    nuGetSource.Key,
                    xElements);
            }
        }
    }
}
