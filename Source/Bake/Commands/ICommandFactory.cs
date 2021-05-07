using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace Bake.Commands
{
    public interface ICommandFactory
    {
        Parser Create(IEnumerable<Type> types);
    }
}
