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

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Bake.Core;
using Bake.ValueObjects;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace Bake.Services
{
    public class SoftwareInstaller : ISoftwareInstaller
    {
        private readonly ILogger<SoftwareInstaller> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDefaults _defaults;
        private readonly ConcurrentDictionary<string, Lazy<Task<InstalledSoftware>>> _alreadyInstalledSoftware = new();

        public SoftwareInstaller(
            ILogger<SoftwareInstaller> logger,
            IHttpClientFactory httpClientFactory,
            IDefaults defaults)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _defaults = defaults;
        }

        public void Install(
            Software software)
        {
            if (!_defaults.InstallSoftwareInBackground)
            {
                _logger.LogInformation(
                    "Installing software in the background disabled, waiting to install {Name} version {Version} to when needed",
                    software.Name,
                    software.Version);
                return;
            }

            // Installs in the background to have software ready when needed
            Task.Run(() => InstallAsync(software, CancellationToken.None));
        }

        public Task<InstalledSoftware> InstallAsync(
            Software software,
            CancellationToken cancellationToken)
        {
            return _alreadyInstalledSoftware.GetOrAdd(
                CreateCacheKey(software),
                _ => new Lazy<Task<InstalledSoftware>>(
                    () => InstallInternalAsync(software, cancellationToken),
                    LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private async Task<InstalledSoftware> InstallInternalAsync(
            Software software,
            CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMinutes(5));

            var directory = Path.Combine(
                Path.GetTempPath(),
                $"bake-software-{Guid.NewGuid():N}");
            var softwareDownloadArchitecture = PickArchitecture();
            var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{software.Name}.exe" : software.Name;
            var filepath = Path.Combine(directory, filename);

            _logger.LogInformation($"Installing {software.Name} v{software.Version} for {softwareDownloadArchitecture} to temp directory {directory}");

            if (!software.Downloads.TryGetValue(softwareDownloadArchitecture, out var url))
            {
                throw new InvalidOperationException($"No download URL found for {software.Name} v{software.Version} for {softwareDownloadArchitecture}");
            }

            var httpClient = _httpClientFactory.CreateClient();
            {
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                response.EnsureSuccessStatusCode(); // TODO: Do something more readable

                await using var filestream = File.OpenWrite(filename);
                await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                await stream.CopyToAsync(filestream, timeout.Token);
            }

            return new InstalledSoftware(
                software,
                filepath);
        }

        private static SoftwareDownloadArchitecture PickArchitecture()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                    case Architecture.X86:
                        return SoftwareDownloadArchitecture.WindowsX86;
                    default:
                        throw new NotSupportedException(
                            $"Unsupported OS architecture {RuntimeInformation.OSArchitecture}");

                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                    case Architecture.X86:
                        return SoftwareDownloadArchitecture.LinuxX86;
                    default:
                        throw new NotSupportedException($"Unsupported OS architecture {RuntimeInformation.OSArchitecture}");
                }

            }

            throw new NotSupportedException($"Unsupported OS {RuntimeInformation.OSDescription}");
        }

        private static string CreateCacheKey(Software software)
        {
            return $"{software.Name} v{software.Version}";
        }
    }
}
