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

using Octokit;
using Octokit.Internal;
using Credentials = Octokit.Credentials;

namespace Bake.Services
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        private static readonly Version Version = typeof(GitHubClientFactory).Assembly.GetName().Version!;

        public Task<IGitHubClient> CreateAsync(
            string token,
            Uri apiUrl,
            CancellationToken cancellationToken)
        {
            var gitHubClient = Create(
                apiUrl,
                new InMemoryCredentialStore(new Credentials(token)));

            return Task.FromResult(gitHubClient);
        }

        private static IGitHubClient Create(
            Uri apiUrl,
            ICredentialStore credentialStore)
        {
            return new GitHubClient(
                new ProductHeaderValue("bake", Version.ToString()),
                credentialStore,
                apiUrl);
        }
    }
}
