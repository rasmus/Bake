using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bake.Services;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class CliTests : TestFor<Cli>
    {
        [Test]
        public async Task ReadStdOut()
        {
            // Arrange
            var command = CreateShellCommand(
                "cmd", null, "/c", "echo", "black", "magic");

            // Act
            var exitCode = await command.ExecuteAsync(CancellationToken.None);

            // Assert
            command.Out.Should().HaveCount(1);
            command.Out.Single().Should().Be("black magic");
        }

        private ICommand CreateShellCommand(
            params string[] arguments)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                arguments = arguments.Prepend("/c").ToArray();
                return Sut.CreateCommand(
                    "cmd", null, arguments);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                arguments = new []
                    {
                        "-c",
                        $"\"{string.Join(" ", arguments)}\""
                    };
                return Sut.CreateCommand(
                    "/bin/bash", null, arguments);
            }

            throw new NotImplementedException();
        }
    }
}
