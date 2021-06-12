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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Core
{
    public class Credentials : ICredentials
    {
        private readonly Lazy<Task<IReadOnlyDictionary<string, string>>> _environmentVariables = new Lazy<Task<IReadOnlyDictionary<string, string>>>(
            GetEnvironmentVariablesAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);

        public async Task<string> GetNuGetApiKeyAsync(
            Uri uri,
            CancellationToken cancellationToken)
        {
            // TODO: Handling of other characters
            var hostname = uri.Host;

            var key = $"bake_credentials_nuget_{hostname}_apikey";
            var environmentVariables = await _environmentVariables.Value;

            if (!environmentVariables.TryGetValue(key, out var value))
            {
                throw new ArgumentException(
                    $"No environment variable named {key} (case insensitive) with credentials");
            }

            return value;
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
