using System;
using System.Collections.Generic;
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
            var app = _commandFactory.Create(commandTypes);

            return await app.ExecuteAsync(args);
        }
    }
}
