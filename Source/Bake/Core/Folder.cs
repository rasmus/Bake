using System;
using System.IO;
using System.Threading.Tasks;

namespace Bake.Core
{
    public class Folder : IAsyncDisposable
    {
        public static Folder New => new Folder(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        public string Path { get; }

        private Folder(
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Path = path;
        }

        public async ValueTask DisposeAsync()
        {
            var start = DateTimeOffset.Now;

            while (DateTimeOffset.Now - start <= TimeSpan.FromMinutes(1))
            {
                try
                {
                    Directory.Delete(Path, true);
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
