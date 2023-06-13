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
using AutoFixture;
using Bake.Core;
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
                .WriteTo.Sink(new LogSink(A<ILogCollector>()))
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
