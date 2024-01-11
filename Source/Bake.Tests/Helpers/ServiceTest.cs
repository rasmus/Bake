// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using Bake.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace Bake.Tests.Helpers
{
    public abstract class ServiceTest<T> : TestProject
        where T : class
    {
        private Lazy<T>? _lazySut;
        private ServiceProvider? _serviceProvider;

        protected T Sut => _lazySut!.Value;
        protected IServiceProvider ServiceProvider => _serviceProvider!;

        protected ServiceTest(string? projectName) : base(projectName)
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
            _serviceProvider?.Dispose();
            _serviceProvider = null;
        }

        protected virtual T CreateSut()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(l => l.AddSerilog(LoggerBuilder.CreateLogger(A<ILogCollector>())));

            Configure(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider(true);

            return _serviceProvider.GetRequiredService<T>();
        }

        protected virtual IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<T>();
        }
    }
}
