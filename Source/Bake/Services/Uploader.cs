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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class Uploader : IUploader
    {
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
        private readonly IReadOnlyDictionary<string, string> MediaTypes = new ConcurrentDictionary<string, string>
        {
            ["bz"] = "application/x-bzip",
            ["bz"] = "application/x-bzip2",
            ["gz"] = "application/gzip",
            ["png"] = "image/png",
            ["zip"] = "application/zip",
            ["tgz"] = "application/gzip",
        };

        private const string DefaultMediaType = "application/octet-stream";

        private readonly ILogger<Uploader> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFileSystem _fileSystem;

        public Uploader(
            ILogger<Uploader> logger,
            IHttpClientFactory httpClientFactory,
            IFileSystem fileSystem)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _fileSystem = fileSystem;
        }

        public async Task UploadAsync(
            string filePath,
            Uri url,
            CancellationToken cancellationToken)
        {
            var file = _fileSystem.Open(filePath);
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath).Trim('.');
            var mediaType = MediaTypes.TryGetValue(fileExtension, out var t) ? t : DefaultMediaType;
            var httpClient = _httpClientFactory.CreateClient();

            _logger.LogInformation(
                "Uploading file {FileName} with extension {Extension} and MIME type {MIME} to URL {Url}",
                fileName, fileExtension, mediaType, url);

            await using var stream = await file.OpenReadAsync(cancellationToken);

            using var multipartFormDataContent = new MultipartFormDataContent();

            var fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            multipartFormDataContent.Add(fileStreamContent, name: "chart", fileName: fileName);

            var response = await httpClient.PostAsync(url, multipartFormDataContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"POST {url} failed with {response.StatusCode}{Environment.NewLine}{content[..(Math.Min(content.Length, 1024) - 1)]}");
            }
        }
    }
}
