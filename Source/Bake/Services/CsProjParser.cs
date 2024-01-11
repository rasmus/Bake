// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using System.Xml.Linq;
using System.Xml.XPath;
using Bake.ValueObjects.DotNet;
using File = System.IO.File;

namespace Bake.Services
{
    public class CsProjParser : ICsProjParser
    {
        private readonly IDotNetTfmParser _dotNetTfmParser;
        
        public CsProjParser(
            IDotNetTfmParser dotNetTfmParser)
        {
            _dotNetTfmParser = dotNetTfmParser;
        }

        public async Task<CsProj> ParseAsync(
            string path,
            CancellationToken cancellationToken)
        {
            var xml = await File.ReadAllTextAsync(
                path,
                cancellationToken);
            var xDocument = XDocument.Parse(xml);

            var packAsTool = ReadBool(xDocument ,"/Project/PropertyGroup/IsTool");
            var isPackable = ReadBool(xDocument, "/Project/PropertyGroup/IsPackable");
            var isPublishable = ReadBool(xDocument, "/Project/PropertyGroup/IsPublishable");
            var toolCommandName = ReadString(xDocument ,"/Project/PropertyGroup/ToolCommandName");
            var assemblyName = ReadString(xDocument, "/Project/PropertyGroup/AssemblyName");
            var targetFrameworkVersions = $"{ReadString(xDocument, "/Project/PropertyGroup/TargetFramework")};{ReadString(xDocument, "/Project/PropertyGroup/TargetFrameworks")}"
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(m => _dotNetTfmParser.TryParse(m, out var v) ? v : null)
                .Where(v => v != null)
                .ToArray();

            return new CsProj(
                packAsTool,
                toolCommandName,
                assemblyName,
                isPackable,
                isPublishable,
                targetFrameworkVersions!);
        }

        private static string ReadString(
            XDocument xDocument,
            string xPath)
        {
            return xDocument.XPathSelectElement(xPath)?.Value ?? string.Empty;
        }

        private static bool ReadBool(
            XDocument xDocument,
            string xPath,
            bool defaultValue = false)
        {
            var value = xDocument.XPathSelectElement(xPath)?.Value;
            return bool.TryParse(value, out var b)
                ? b
                : defaultValue;
        }
    }
}
