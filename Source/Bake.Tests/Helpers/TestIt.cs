using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;

// ReSharper disable AssignNullToNotNullAttribute

namespace Bake.Tests.Helpers
{
    public abstract class TestIt
    {
        private List<string> _filesToDelete;

        protected IFixture Fixture { get; private set; }

        [SetUp]
        public void SetUpTestIt()
        {
            _filesToDelete = new List<string>();

            Fixture = new Fixture().Customize(new AutoMoqCustomization());
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
            return new Mock<T>().Object;
        }

        protected T Inject<T>(T instance)
            where T : class
        {
            Fixture.Inject(instance);
            return instance;
        }

        protected Mock<T> InjectMock<T>(params object[] args)
            where T : class
        {
            var mock = new Mock<T>(args);
            Fixture.Inject(mock.Object);
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
            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync();
        }

        protected async Task<string> WriteEmbeddedAsync(
            string fileEnding)
        {
            var content = await ReadEmbeddedAsync(fileEnding);
            var path = Path.GetTempFileName();
            await File.WriteAllTextAsync(path, content);
            _filesToDelete.Add(path);
            return path;
        }
    }
}
