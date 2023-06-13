// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bake.Tests.Helpers;
using NUnit.Framework;

namespace Bake.Tests
{
    public class LicenseHeader : TestIt
    {
        private static readonly Regex LicenseHeaderFinder = new(
            @"^\s*\/\/ MIT License.+(?:\n\s*\/\/.*)+\s*",
            RegexOptions.Compiled);

        [Test, Explicit]
        public async Task UpdateHeaders()
        {
            // Arrange
            var currentHeader = await ReadEmbeddedAsync("license.txt");

            // Act
            var files = Directory.GetFiles(
                ProjectHelper.GetRoot(),
                "*.cs",
                SearchOption.AllDirectories);

            await Task.WhenAll(files.Select(f => UpdateHeaderAsync(f, currentHeader, CancellationToken.None)));

            Console.WriteLine($"Updated {files.Length} files");
        }

        private static async Task UpdateHeaderAsync(
            string path,
            string licenseHeader,
            CancellationToken cancellationToken)
        {
            var content = await File.ReadAllTextAsync(path, cancellationToken);
            content = content.Trim();

            if (LicenseHeaderFinder.IsMatch(content))
            {
                content = new StringBuilder()
                    .AppendLine(LicenseHeaderFinder.Replace(content, licenseHeader))
                    .ToString();
            }
            else
            {
                content = new StringBuilder()
                    .Append(licenseHeader)
                    .AppendLine(content)
                    .ToString();
            }

            await File.WriteAllTextAsync(path, content, cancellationToken);
        }
    }
}
