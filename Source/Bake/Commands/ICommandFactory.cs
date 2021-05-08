using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace Bake.Commands
{
    public interface ICommandFactory
    {
        CommandLineApplication Create(IEnumerable<Type> types);
    }
}
