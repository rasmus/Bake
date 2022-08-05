// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects;

namespace Bake.Services
{
    public class DockerLabels : IDockerLabels
    {
        public async Task<IReadOnlyDictionary<string, string>> FromIngredientsAsync(
            Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            async Task<T> GetAsync<T>(Func<Task<T>> getter)
                where T : class
            {
                try
                {
                    return await getter();
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
            }

            var git = await GetAsync(() => ingredients.GitTask);
            var gitHub = await GetAsync(() => ingredients.GitHubTask);
            var version = ingredients.Version;
            var now = DateTimeOffset.Now;

            var labels = new Dictionary<string, string>
                {
                    ["version"] = version.ToString(),
                    ["build.time"] = now.ToString("O"),
                    ["build.timestamp"] = now.ToUnixTimeSeconds().ToString(),
                };

            if (git != null)
            {
                labels["git.sha"] = git.Sha;
                labels["git.origin"] = git.OriginUrl.AbsoluteUri;
            }

            if (gitHub != null)
            {
                labels["github.owner"] = gitHub.Owner;
                labels["github.repository"] = gitHub.Repository;
                labels["github.url"] = gitHub.Url.AbsoluteUri;
            }

            return labels;
        }

        public string Serialize(IReadOnlyDictionary<string, string> labels)
        {
            var lines = labels
                .Select(kv => $"LABEL \"{kv.Key}\"=\"{kv.Value}\"");
            return string.Join(Environment.NewLine, lines);
        }
    }
}
