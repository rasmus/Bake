using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public abstract class TestIt
    {
        protected IFixture Fixture { get; private set; }

        [SetUp]
        public void SetUpTestIt()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
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
    }
}
