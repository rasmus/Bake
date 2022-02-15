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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using HashAlgorithm = Bake.ValueObjects.HashAlgorithm;

namespace Bake.Core
{
    public class File : IFile
    {
        public string Path { get; }
        public string FileName => System.IO.Path.GetFileName(Path);
        public long Size => new FileInfo(Path).Length;

        public File(string path)
        {
            Path = path;
        }

        public Task<Stream> OpenWriteAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(System.IO.File.Open(
                Path,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None));
        }

        public Task<Stream> OpenReadAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(
                new BufferedStream(
                    System.IO.File.Open(
                        Path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read),
                    1200000));
        }

        public async Task<string> GetHashAsync(
            HashAlgorithm hashAlgorithm,
            CancellationToken cancellationToken)
        {
            if (hashAlgorithm != HashAlgorithm.SHA256)
            {
                throw new ArgumentOutOfRangeException(nameof(hashAlgorithm));
            }

            await using var stream = await OpenReadAsync(cancellationToken);

            using var sha256 = SHA256.Create();
            var checksum = await sha256.ComputeHashAsync(stream, cancellationToken);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        public void Dispose()
        {
            System.IO.File.Delete(Path);
        }
    }
}
