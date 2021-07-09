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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Bake.ValueObjects;

namespace Bake.Core
{
    public class NuGetConfiguration : INuGetConfiguration
    {
        public string Generate(IReadOnlyCollection<NuGetSource> nuGetSources)
        {
            var xDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("configuration",
                    new XElement("packageSources", GeneratePackageSources(nuGetSources)),
                    new XElement("packageSourceCredentials", GeneratePackageSourceCredentials(nuGetSources))));

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

        private static IEnumerable<XElement> GeneratePackageSourceCredentials(
            IReadOnlyCollection<NuGetSource> nuGetSources)
        {
            if (nuGetSources.All(s => string.IsNullOrEmpty(s.ClearTextPassword)))
            {
                yield break;
            }

            foreach (var nuGetSource in nuGetSources)
            {
                yield return new XElement(
                    nuGetSource.Key,
                    new XElement("add", new XAttribute("key", "Username"), new XAttribute("value", "USERNAME")),
                    new XElement("add", new XAttribute("key", "ClearTextPassword"), new XAttribute("value", nuGetSource.ClearTextPassword)));
            }
        }
    }
}

/*

<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="github" value="https://nuget.pkg.github.com/OWNER/index.json" />
    </packageSources>
    <packageSourceCredentials>
        <github>
            <add key="Username" value="USERNAME" />
            <add key="ClearTextPassword" value="TOKEN" />
        </github>
    </packageSourceCredentials>
</configuration>


XNamespace xns = "http://www.fruitauthority.fake";
XDocument xDoc = 
    new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
        new XElement(xns + "FruitBasket",
            new XElement(xns + "Fruit",
                new XElement(xns + "FruitName", "Banana"),
                new XElement(xns + "FruitColor", "Yellow")),
            new XElement(xns + "Fruit",
                new XElement(xns + "FruitName", "Apple"),
                new XElement(xns + "FruitColor", "Red"))
                ));
 */
