using System;
using System.Collections.Generic;
using System.Text;

namespace Bake.Services
{
    interface ICli
    {
        ICommand CreateCommand(
            string command,
            string workingDirectory,
            params string[] arguments);
    }
}
