using System;
using AutoFixture;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public class TestFor<T> : TestIt
    {
        private Lazy<T> _lazySut;
        protected T Sut => _lazySut.Value;

        [SetUp]
        public void SetUpTestsFor()
        {
            _lazySut = new Lazy<T>(CreateSut);
        }

        protected virtual T CreateSut()
        {
            return Fixture.Create<T>();
        }
    }
}
