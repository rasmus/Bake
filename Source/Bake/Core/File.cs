using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Core
{
    public class File : IFile
    {
        public string Path { get; }

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

        public void Dispose()
        {
            System.IO.File.Delete(Path);
        }
    }
}
