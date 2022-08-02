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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;

namespace Bake.Services
{
    public class Uploader : IUploader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFileSystem _fileSystem;

        public Uploader(
            IHttpClientFactory httpClientFactory,
            IFileSystem fileSystem)
        {
            _httpClientFactory = httpClientFactory;
            _fileSystem = fileSystem;
        }

        public async Task UploadAsync(
            string filePath,
            Uri url,
            CancellationToken cancellationToken)
        {
            var file = _fileSystem.Open(filePath);
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
                                FileName = Path.GetFileName(filePath)
                            }
                        }
                    }
                }
            };

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"POST {url} failed with {response.StatusCode}");
            }
        }
    }
}
