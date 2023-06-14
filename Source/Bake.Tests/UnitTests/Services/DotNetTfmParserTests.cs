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

using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects.DotNet;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class DotNetTfmParserTests : TestFor<DotNetTfmParser>
    {
        // .NET Standard
        [TestCase(
            "netstandard2.1",
            "2.1.0",
            TargetFramework.NetStandard)]

        // .NET Framework
        [TestCase(
            "net35",
            "3.5.0",
            TargetFramework.NetFramework)]
        [TestCase(
            "net403",
            "4.0.3",
            TargetFramework.NetFramework)]
        [TestCase(
            "net452",
            "4.5.2",
            TargetFramework.NetFramework)]
        [TestCase(
            "net48",
            "4.8.0",
            TargetFramework.NetFramework)]

        // .NET
        [TestCase(
            "netcoreapp2.0",
            "2.0.0",
            TargetFramework.Net)]
        [TestCase(
            "netcoreapp3.1",
            "3.1.0",
            TargetFramework.Net)]
        [TestCase(
            "net5.0",
            "5.0.0",
            TargetFramework.Net)]
        public void Success(
            string moniker,
            string expectedVersion,
            TargetFramework expectedTargetFramework)
        {
            // Act
            Sut.TryParse(moniker, out var targetFrameworkVersion).Should().BeTrue();

            // Assert
            targetFrameworkVersion.Should().NotBeNull();
            targetFrameworkVersion.Moniker.Should().Be(moniker);
            targetFrameworkVersion.Version.ToString().Should().Be(expectedVersion);
            targetFrameworkVersion.Framework.Should().Be(expectedTargetFramework);
        }
    }
}
