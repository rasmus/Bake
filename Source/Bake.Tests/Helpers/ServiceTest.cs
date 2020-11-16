﻿using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public abstract class ServiceTest<T> : TestProject
        where T : class
    {
        private Lazy<T> _lazySut;
        private ServiceProvider _serviceProvider;

        protected T Sut => _lazySut.Value;
        protected IServiceProvider ServiceProvider => _serviceProvider;

        protected ServiceTest(string projectName) : base(projectName)
        {
        }

        [SetUp]
        public void SetUpServiceTest()
        {
            _lazySut = new Lazy<T>(CreateSut, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        [TearDown]
        public void TearDownServiceTest()
        {
            _serviceProvider.Dispose();
            _serviceProvider = null;
        }

        protected virtual T CreateSut()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(l => l.AddConsole());

            Configure(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider(true);

            return _serviceProvider.GetRequiredService<T>();
        }

        protected virtual void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddTransient<T>();
        }
    }
}
