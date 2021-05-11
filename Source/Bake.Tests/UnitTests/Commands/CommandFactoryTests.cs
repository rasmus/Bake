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

using System.Threading;
using System.Threading.Tasks;
using Bake.Commands;
using Bake.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ICommand = Bake.Commands.ICommand;

namespace Bake.Tests.UnitTests.Commands
{
    public class CommandFactoryTests : TestFor<CommandFactory>
    {
        [CommandVerb("A")]
        public class CommandA : ICommand
        {
            // ReSharper disable once UnusedMember.Global
            public Task<int> ExecuteAsync(
                string name = "hej",
                int magicNumber = 42,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
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
                    "--magic-number=42"
                });

            // Assert
            result.Should().Be(0);
        }
    }
}
