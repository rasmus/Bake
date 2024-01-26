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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Core
{
    public class EnvironmentVariables : IEnvironmentVariables
    {
        private readonly Lazy<Task<IReadOnlyDictionary<string, string>>> _environmentVariables = new(
            GetEnvironmentVariablesAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);

        public Task<IReadOnlyDictionary<string, string>> GetAsync(CancellationToken _)
        {
            return _environmentVariables.Value;
        }

        private static Task<IReadOnlyDictionary<string, string>> GetEnvironmentVariablesAsync()
        {
            return Task.Factory.StartNew(
                () =>
                {
                    var environmentVariables = Environment.GetEnvironmentVariables();
                    var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DictionaryEntry dictionaryEntry in environmentVariables)
                    {
                        var key = dictionaryEntry.Key as string;
                        var value = dictionaryEntry.Value as string;
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            dictionary[key] = value;
                        }
                    }

                    return (IReadOnlyDictionary<string, string>) dictionary;
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
    }
}
