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

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable AssignNullToNotNullAttribute

namespace Bake.Tests.Helpers
{
    public abstract class TestIt
    {
        private List<string> _filesToDelete = null!;

        protected IFixture Fixture { get; private set; } = null!;

        [SetUp]
        public void SetUpTestIt()
        {
            _filesToDelete = new List<string>();

            Fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }

        [TearDown]
        public void TearDownTestIt()
        {
            foreach (var file in _filesToDelete)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        protected T A<T>()
        {
            return Fixture.Create<T>();
        }

        protected List<T> Many<T>(int count = 3)
        {
            return Fixture.CreateMany<T>(count).ToList();
        }

        protected T Mock<T>()
            where T : class
        {
            return Substitute.For<T>();
        }

        protected T Inject<T>(T instance)
            where T : class
        {
            Fixture.Inject(instance);
            return instance;
        }

        protected T InjectMock<T>(params object[] args)
            where T : class
        {
            var mock = Substitute.For<T>(args);
            Fixture.Inject(mock);
            return mock;
        }

        protected async Task<string> ReadEmbeddedAsync(
            string fileEnding)
        {
            var assembly = GetType().Assembly;
            var resourceNames = assembly.GetManifestResourceNames()
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var resourceName = resourceNames.Single(n => n.EndsWith(fileEnding, StringComparison.OrdinalIgnoreCase));
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(stream!);
            return await streamReader.ReadToEndAsync();
        }

        protected string Lines(
            params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        protected async Task<string> WriteEmbeddedAsync(
            string fileEnding)
        {
            var content = await ReadEmbeddedAsync(fileEnding);
            var path = Path.GetTempFileName();
            await System.IO.File.WriteAllTextAsync(path, content);
            _filesToDelete.Add(path);
            return path;
        }
    }
}
