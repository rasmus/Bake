using System.Threading.Tasks;
using Bake.Commands;
using Bake.Tests.Helpers;
using NUnit.Framework;
using ICommand = Bake.Commands.ICommand;

namespace Bake.Tests.UnitTests.Commands
{
    public class CommandFactoryTests : TestFor<CommandFactory>
    {
        [CommandVerb("A")]
        public class CommandA : ICommand
        {
            public Task<int> ExecuteAsync(string name, int magicNumber)
            {
                return Task.FromResult(0);
            }
        }

        [Test]
        public void A()
        {
            // Act
            var rootCommand = Sut.Create(new[] {typeof(CommandA)});

            // Assert

        }
    }
}
