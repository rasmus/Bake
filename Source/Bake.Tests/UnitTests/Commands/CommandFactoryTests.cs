using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Bake.Commands;
using Bake.Core;
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
            public Task<int> ExecuteAsync(
                string name,
                int magicNumber,
                SemVer version,
                CancellationToken cancellationToken)
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
            var parser = Sut.Create(new[] {typeof(CommandA)});

            // Assert
            var result = await parser.InvokeAsync(
                new[]
                {
                    "A",
                    "--name", "magic",
                    "--magic-number", "42"
                });

            // Assert
            result.Should().Be(0);
        }
    }
}
