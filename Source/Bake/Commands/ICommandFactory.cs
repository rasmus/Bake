using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Bake.Commands
{
    public interface ICommandFactory
    {
        RootCommand Create(IEnumerable<Type> types);
    }
}
