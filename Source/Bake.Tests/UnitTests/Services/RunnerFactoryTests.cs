﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bake.Extensions;
using Bake.Services;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class RunnerFactoryTests : TestFor<RunnerFactory>
    {
        [Test]
        public async Task ReadStdOut()
        {
            // Arrange
            var command = CreateShellCommand(
                "echo", "black", "magic");

            // Act
            var exitCode = await command.ExecuteAsync(CancellationToken.None);

            // Assert
            exitCode.Should().Be(0);
            var output = command.NonEmptyOut().ToList();
            output.Should().HaveCount(1);
            output.Single().Should().Be("black magic");
        }

        private IRunner CreateShellCommand(
            params string[] arguments)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                arguments = arguments.Prepend("/c").ToArray();
                return Sut.CreateRunner(
                    "cmd", null, arguments);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                arguments = new []
                    {
                        "-c",
                        $"\"{string.Join(" ", arguments)}\""
                    };
                return Sut.CreateRunner(
                    "/bin/bash", null, arguments);
            }

            throw new NotImplementedException();
        }
    }
}