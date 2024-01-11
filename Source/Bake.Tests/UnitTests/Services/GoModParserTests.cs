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

using Bake.Services;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.UnitTests.Services
{
    public class GoModParserTests : TestFor<GoModParser>
    {
        [TestCase(
            "module mymodule",
            "mymodule",
            "")]
        [TestCase(
            "module mymodule/v3",
            "mymodule",
            "3")]
        [TestCase(
            "module my.module/v3",
            "my.module",
            "3")]
        [TestCase(
            "module my-module/v3",
            "my-module",
            "3")]
        [TestCase(
            "module example.com/mymodule",
            "mymodule",
            "")]
        [TestCase(
            "module example.com/mymodule/v2",
            "mymodule",
            "2")]
        [TestCase(
            "module example.com/path/mymodule/v2",
            "mymodule",
            "2")]
        [TestCase(
            "module example.com/path/my.module/v2",
            "my.module",
            "2")]
        public void Success(string str, string expectedName, string expectedVersion)
        {
            // Act
            Sut.TryParse(str, out var goModuleName).Should().BeTrue();

            // Assert
            goModuleName!.Name.Should().Be(expectedName);
            goModuleName.Version.Should().Be(expectedVersion);
        }
    }
}
