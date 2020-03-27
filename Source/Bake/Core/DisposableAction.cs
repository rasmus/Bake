using System;
using System.Threading;

namespace Bake.Core
{
    public class DisposableAction : IDisposable
    {
        public static IDisposable With(Action action) => new DisposableAction(action);

        private Action _action;

        private DisposableAction(
            Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null)?.Invoke();
        }
    }
}
