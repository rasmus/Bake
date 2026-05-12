// MIT License
// 
// Copyright (c) 2021-2026 Rasmus Mikkelsen
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

using Bake.Core;
using Bake.Tests.Helpers;
using NUnit.Framework;
using Shouldly;

namespace Bake.Tests.UnitTests.Core
{
    public class EnvironmentVariablesTests : TestFor<EnvironmentVariables>
    {
        [Test]
        public async Task GetAsync()
        {
            // Arrange
            var expectedEnvironmentVariables = new Dictionary<string, string>
                {
                    [A<string>()] = A<string>(),
                    [A<string>()] = A<string>(),
                    [A<string>()] = A<string>(),
                };
            IReadOnlyDictionary<string, string> environmentVariables;

            // Act
            using (SetEnvironmentVariables(expectedEnvironmentVariables))
            {
                environmentVariables = await Sut.GetAsync(CancellationToken.None);
            }

            // Assert
            foreach (var (expectedKey, expectedValue) in expectedEnvironmentVariables)
            {
                environmentVariables.TryGetValue(expectedKey, out var value).ShouldBeTrue();
                value.ShouldBe(expectedValue);
            }
        }

        private static IDisposable SetEnvironmentVariables(
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            var originalEnvironmentVariables = environmentVariables
                .ToDictionary(kv => kv.Key, kv => Environment.GetEnvironmentVariable(kv.Key));

            foreach (var (key, value) in environmentVariables)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            return new DisposableAction(() =>
            {
                foreach (var (key, value) in originalEnvironmentVariables)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            });
        }
    }
}
