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

using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class PlatformParserTests : TestFor<PlatformParser>
    {
        [TestCase("win/x86", ExecutableOperatingSystem.Windows, ExecutableArchitecture.Intel32)]
        [TestCase("linux/x64", ExecutableOperatingSystem.Linux, ExecutableArchitecture.Intel64)]
        public void Success(
            string str,
            ExecutableOperatingSystem executableOs,
            ExecutableArchitecture expectedArch)
        {
            // Act
            Sut.TryParse(str, out var targetPlatform).Should().BeTrue();

            // Assert
            targetPlatform.Os.Should().Be(executableOs);
            targetPlatform.Arch.Should().Be(expectedArch);
        }
    }
}
