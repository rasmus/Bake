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
using System.Threading;
using System.Threading.Tasks;
using Bake.Commands;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects.Destinations;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ICommand = Bake.Commands.ICommand;

namespace Bake.Tests.UnitTests.Commands
{
    public class CommandFactoryTests : TestFor<CommandFactory>
    {
        [SetUp]
        public void SetUp()
        {
            Inject<IDestinationParser>(new DestinationParser(new Defaults(TestEnvironmentVariables.None)));
        }

        [Command("A", "B")]
        public class CommandA : ICommand
        {
            // ReSharper disable once UnusedMember.Global
            public Task<int> ExecuteAsync(
                string name = "hej",
                int exitCode = 42,
                bool throwException = false,
                CancellationToken cancellationToken = default,
                Destination[] destination = null)
            {
                if (throwException) throw new Exception();

                return Task.FromResult(exitCode);
            }
        }

        protected override IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return base.Configure(serviceCollection)
                .AddTransient<CommandA>();
        }

        [Test]
        public async Task TestCreation()
        {
            // Act
            var app = Sut.Create(new[] {typeof(CommandA)});

            // Assert
            var result = await app.ExecuteAsync(
                new[]
                {
                    "A",
                    "--name=magic",
                    "--exit-code=42",
                    "--destination=nuget,nuget>http://localhost:5555/v3/index.json",
                });

            // Assert
            result.Should().Be(42);
        }

        [Test]
        public async Task ThrowException()
        {
            // Act
            var app = Sut.Create(new[] {typeof(CommandA)});

            // Assert
            var result = await app.ExecuteAsync(
                new[]
                {
                    "A",
                    "--throw-exception=true",
                });

            // Assert
            result.Should().Be(ExitCodes.Core.UnexpectedError);
        }
    }
}
