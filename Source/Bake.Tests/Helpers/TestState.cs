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

using Microsoft.Extensions.DependencyInjection;

namespace Bake.Tests.Helpers
{
    public class TestState
    {
        public static TestState New(params string[] args) => new(
            args,
            null,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        public string[] Arguments { get; }
        public Action<IServiceCollection>? Overrides { get; }
        public IReadOnlyDictionary<string, string> EnvironmentVariables { get; }

        private TestState(
            string[] arguments,
            Action<IServiceCollection>? overrides,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            Arguments = arguments;
            Overrides = overrides;
            EnvironmentVariables = environmentVariables;
        }

        public TestState WithOverrides(Action<IServiceCollection> overrides)
        {
            if (Overrides != null)
            {
                throw new InvalidOperationException("Overrides already configured");
            }

            return new TestState(
                Arguments,
                overrides,
                EnvironmentVariables);
        }

        public TestState WithEnvironmentVariable(
            string key,
            string value)
        {
            return WithEnvironmentVariables(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [key] = value
                });
        }

        public TestState WithEnvironmentVariables(
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            var intersects = EnvironmentVariables.Keys
                .Intersect(environmentVariables.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (intersects.Any())
            {
                throw new ArgumentException(
                    $"The environment variables '{string.Join(", ", intersects)}' are already defined");
            }

            var updatedEnvironmentVariables = Enumerable.Empty<KeyValuePair<string, string>>()
                .Concat(EnvironmentVariables)
                .Concat(environmentVariables)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value,
                    StringComparer.OrdinalIgnoreCase);

            return new TestState(
                Arguments,
                Overrides,
                updatedEnvironmentVariables);
        }
    }
}
