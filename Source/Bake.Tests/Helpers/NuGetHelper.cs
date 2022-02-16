// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Packaging;

namespace Bake.Tests.Helpers
{
    public static class NuGetHelper
    {
        public static async Task<NuSpec> LoadAsync(
            string packagePath)
        {
            // https://docs.microsoft.com/en-us/nuget/reference/nuget-client-sdk#read-a-package

            await using var fileStream = File.Open(packagePath, FileMode.Open);
            using var packageArchiveReader = new PackageArchiveReader(fileStream);
            var nuSpecReader = packageArchiveReader.NuspecReader;

            Console.WriteLine(new string('=', 42));
            Console.WriteLine(nuSpecReader.Xml.ToString(SaveOptions.None));
            Console.WriteLine(new string('=', 42));

            var repositoryMetadata = nuSpecReader.GetRepositoryMetadata();

            return new NuSpec(
                nuSpecReader.GetId(),
                repositoryMetadata?.Url,
                repositoryMetadata?.Commit);
        }

        public class NuSpec
        {
            public string Id { get; }
            public string RepositoryCommit { get; }
            public string RepositoryUrl { get; }

            public NuSpec(
                string id,
                string? repositoryUrl,
                string? repositoryCommit)
            {
                Id = id;
                RepositoryCommit = repositoryCommit ?? string.Empty;
                RepositoryUrl = repositoryUrl ?? string.Empty;
            }
        }
    }
}
