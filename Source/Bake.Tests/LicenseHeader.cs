using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bake.Tests.Helpers;
using NUnit.Framework;

namespace Bake.Tests
{
    public class LicenseHeader : TestIt
    {
        private static readonly Regex LicenseHeaderFinder = new Regex(
            @"^\s+\/\*.*?\*\/",
            RegexOptions.Compiled);

        [Test]
        public async Task UpdateHeaders()
        {
            // Arrange
            var currentHeader = ReadEmbeddedAsync("license.txt");

            // 

            var files = GetFiles().ToList();
        }

        private static IEnumerable<string> GetFiles()
        {
            var uri = new Uri(typeof(LicenseHeader).Assembly.CodeBase);
            var directory = Path.GetDirectoryName(uri.LocalPath);

            while (true)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    throw new Exception("Could not find repository root");
                }

                var isRepositoryRoot = Directory.GetFiles(directory)
                    .Select(Path.GetFileName)
                    .Any(f => string.Equals(f, "README.md"));
                if (isRepositoryRoot)
                {
                    break;
                }

                directory = Path.GetDirectoryName(directory);
            }

            return Directory.GetFiles(
                directory,
                "*.cs",
                SearchOption.AllDirectories);
        }
    }
}
