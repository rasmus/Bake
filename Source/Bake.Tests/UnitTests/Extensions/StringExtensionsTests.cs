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

using Bake.Extensions;
using Bake.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.UnitTests.Extensions
{
    public class StringExtensionsTests : TestIt
    {
        [TestCase("\u00E6\u00F8\u00E5", "aoa")]
        [TestCase("abc", "abc")]
        [TestCase("kjcasd )=(/=)(/& cads\u00C6\u00D8\u00C5\u00C6hcadjskchj/(/7856", "kjcasd-cadsaoaahcadjskchj-7856")]
        public void ToSlug(string input, string expectedOutput)
        {
            // Act
            var result = input.ToSlug();

            // Assert
            result.Should().Be(expectedOutput);
        }
    }
}
