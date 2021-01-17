using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Bake.Commands;

namespace Bake
{
    public class Executor : IExecutor
    {
        private readonly ICommandFactory _commandFactory;

        public Executor(
            ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }

        public async Task<int> ExecuteAsync(string[] args, IReadOnlyCollection<Type> commandTypes)
        {
            var rootCommand = _commandFactory.Create(commandTypes);
            var commandLineBuilder = new CommandLineBuilder(rootCommand)
                .UseHelp()
                .UseVersionOption()
                .UseMiddleware(Middleware);

            var parser = commandLineBuilder.Build();

            return await parser.InvokeAsync(args);
        }

        private Task Middleware(InvocationContext context, Func<InvocationContext, Task> next)
        {

            return next(context);
        }
    }
}
