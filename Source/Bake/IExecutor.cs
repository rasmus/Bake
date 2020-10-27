using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bake
{
    public interface IExecutor
    {
        Task<int> ExecuteAsync(string[] args, IReadOnlyCollection<Type> commandTypes);
    }
}
