using System;
using Serilog.Core;
using Serilog.Events;

namespace Bake.Core
{
    public class LogSink : ILogEventSink
    {
        private readonly object _syncRoot = new object();

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();

            lock (_syncRoot)
            {
                Console.WriteLine(message);
            }
        }
    }
}
