using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Core
{
    public interface IFile : IDisposable
    {
        string Path { get; }

        Task<Stream> OpenWriteAsync(
            CancellationToken cancellationToken);
    }
}
