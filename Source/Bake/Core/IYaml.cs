using System.Threading;
using System.Threading.Tasks;

namespace Bake.Core
{
    public interface IYaml
    {
        Task<string> SerializeAsync<T>(
            T obj,
            CancellationToken cancellationToken);

        Task<T> DeserializeAsync<T>(
            string yaml,
            CancellationToken cancellationToken);
    }
}
