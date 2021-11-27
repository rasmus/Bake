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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public abstract class TestProject : TestIt
    {
        protected string ProjectName { get; }
        protected string WorkingDirectory => _folder.Path;
        private string _previousCurrentDirectory;

        private Folder _folder;

        protected TestProject(
            string projectName)
        {
            ProjectName = projectName;
        }

        [SetUp]
        public void SetUpTestProject()
        {
            _folder = Folder.New;

            GitHelper.Create(_folder.Path);

            DirectoryCopy(
                Path.Combine(
                    ProjectHelper.GetRoot(),
                    "TestProjects",
                    ProjectName),
                _folder.Path);

            _previousCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(WorkingDirectory);
        }

        [TearDown]
        public async Task TearDownTestProject()
        {
            Directory.SetCurrentDirectory(_previousCurrentDirectory);
            await _folder.DisposeAsync();
        }

        protected string AssertFileExists(long minimumSize, params string[] path)
        {
            var filePath = AssertFileExists(path);
            var fileInfo = new FileInfo(filePath);

            fileInfo.Length.Should().BeGreaterThan(minimumSize, $"File {fileInfo} should be at least {minimumSize.BytesToString()}, but is only {fileInfo.Length.BytesToString()}");
            
            Console.WriteLine($"File {filePath} of size {fileInfo.Length.BytesToString()} is a minimum of {minimumSize.BytesToString()}");

            return filePath;
        }

        protected string AssertFileExists(params string[] path)
        {
            var filePath = path.Aggregate(
                WorkingDirectory,
                Path.Combine);

            System.IO.File.Exists(filePath).Should().BeTrue(
                $"'{Path.GetFileName(filePath)}' should be among: {Environment.NewLine}{PrettyPrintDirectory(Path.GetDirectoryName(filePath))}'");

            Console.WriteLine($"File {filePath} exists");

            return filePath;
        }

        protected static async Task AssertContainerPingsAsync(
            string expectedImage,
            int port,
            IReadOnlyDictionary<string, string> environmentVariables = null)
        {
            var hostPort = SocketHelper.FreeTcpPort();
            var url = $"http://localhost:{hostPort}/ping";
            using var _ = await DockerHelper.RunAsync(
                expectedImage,
                new Dictionary<int, int> { [port] = hostPort },
                environmentVariables ?? new Dictionary<string, string>(),
                CancellationToken.None);
            using var httpClient = new HttpClient();
            var start = Stopwatch.StartNew();
            var success = false;
            while (start.Elapsed < TimeSpan.FromSeconds(15))
            {
                try
                {
                    using var response = await httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Status code {response.StatusCode}");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Got content: {Environment.NewLine}{content}");

                    success = true;
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed with {e.GetType().Name}: {e.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            success.Should().BeTrue();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            var dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (var subDirectory in dirs)
            {
                var tempPath = Path.Combine(destDirName, subDirectory.Name);
                DirectoryCopy(subDirectory.FullName, tempPath);
            }
        }

        private static string PrettyPrintDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return $"non-existing directory: {path}";
            }

            var directoriesInDirectory = Directory.GetDirectories(path)
                .Select(Path.GetFileName)
                .OrderBy(n => n);

            var filesInDirectory = Directory.GetFiles(path)
                .Select(f => new
                    {
                        name = Path.GetFileName(f),
                        size = new FileInfo(f).Length.BytesToString()
                    })
                .OrderBy(a => a.name)
                .Select(a => $"{a.size,10} {a.name}");

            var lines = Enumerable.Empty<string>()
                .Concat(directoriesInDirectory.Select(d => $"D: {d}"))
                .Concat(filesInDirectory.Select(f => $"F: {f}"));

            return string.Join(Environment.NewLine, lines);
        }
    }
}
