using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bake.Services
{
    public interface ICommand : IAsyncDisposable
    {
        Task<int> ExecuteAsync(CancellationToken cancellationToken);
        IObservable<string> StdOut { get; }
        IObservable<string> StdErr { get; }
        IReadOnlyCollection<string> Out { get; }
        IReadOnlyCollection<string> Err { get; }
    }
}
