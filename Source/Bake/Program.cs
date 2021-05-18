// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
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

using System.Linq;
using System.Threading.Tasks;
using Bake.Commands;
using Bake.Core;
using Bake.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

// ReSharper disable AccessToDisposedClosure

namespace Bake
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var logCollector = new LogCollector();
            var serviceCollection = new ServiceCollection()
                .AddLogging(f => f.AddSerilog(LoggerBuilder.CreateLogger(logCollector)))
                .AddBake(logCollector);

            var commandType = typeof(ICommand);
            var commandTypes = serviceCollection
                .Where(d => commandType.IsAssignableFrom(d.ImplementationType))
                .Select(d => d.ImplementationType)
                .ToList();

            await using var serviceProvider = serviceCollection.BuildServiceProvider(true);

            var executor = serviceProvider.GetRequiredService<IExecutor>();
            var returnCode = await executor.ExecuteAsync(args, commandTypes);

            return returnCode;
        }
    }
}
