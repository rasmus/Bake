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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects.Recipes.OctopusDeploy;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Cooks.OctopusDeploy
{
    public class OctopusDeployPackagePushCook : Cook<OctopusDeployPackagePushRecipe>
    {
        private readonly ILogger<OctopusDeployPackagePushCook> _logger;
        private readonly ICredentials _credentials;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFileSystem _fileSystem;

        public OctopusDeployPackagePushCook(
            ILogger<OctopusDeployPackagePushCook> logger,
            ICredentials credentials,
            IHttpClientFactory httpClientFactory,
            IFileSystem fileSystem)
        {
            _logger = logger;
            _credentials = credentials;
            _httpClientFactory = httpClientFactory;
            _fileSystem = fileSystem;
        }

        protected override async Task<bool> CookAsync(
            IContext context,
            OctopusDeployPackagePushRecipe recipe,
            CancellationToken cancellationToken)
        {
            var apiKey = await _credentials.TryGetOctopusDeployApiKeyAsync(
                recipe.Url,
                cancellationToken);
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogCritical(
                    "Failed to get an API key for {Url}",
                    recipe.Url);
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient();
            await Task.WhenAll(recipe.Packages.Select(p => UploadAsync(
                p,
                apiKey,
                recipe.Url,
                httpClient,
                cancellationToken)));
            return true;
        }

        private async Task UploadAsync(
            string packagePath,
            string apiKey,
            Uri url,
            HttpClient httpClient,
            CancellationToken cancellationToken)
        {
            url = new Uri(url, "/api/packages/raw?replace=false");
            var file = _fileSystem.Open(packagePath);
            await using var stream = await file.OpenReadAsync(cancellationToken);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new MultipartFormDataContent
                    {
                        new StreamContent(stream)
                        {
                            Headers =
                            {
                                ContentType = new MediaTypeHeaderValue("multipart/form-data"),
                                ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                {
                                    Name = "fileData",
                                    FileName = Path.GetFileName(packagePath)
                                }
                            }
                        }
                    },
                    Headers =
                    {
                        {"X-Octopus-ApiKey", apiKey}
                    }
                };
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"POST {url} failed with {response.StatusCode}");
            }
        }
    }
}
