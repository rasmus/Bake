using System;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using Serilog.Core;

namespace Bake.Tests.Helpers
{
    public class TestFor<T> : TestIt
    {
        private Lazy<T> _lazySut;
        private ServiceProvider _serviceProvider;
        private Logger _logger;
        protected T Sut => _lazySut.Value;
        
        [SetUp]
        public void SetUpTestFor()
        {
            _lazySut = new Lazy<T>(CreateSut);
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
            _serviceProvider = Configure(new ServiceCollection())
                .AddLogging(b => b.AddSerilog(_logger))
                .BuildServiceProvider();

            Inject(_serviceProvider.GetRequiredService<ILogger<T>>());
            Inject<IServiceProvider>(_serviceProvider);
        }

        [TearDown]
        public void TearDownTestFor()
        {
            _serviceProvider.Dispose();
            _logger.Dispose();
        }

        protected virtual IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }

        protected virtual T CreateSut()
        {
            return Fixture.Create<T>();
        }
    }
}
