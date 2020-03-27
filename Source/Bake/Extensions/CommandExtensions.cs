using System.Collections.Generic;
using System.Linq;
using Bake.Services;

namespace Bake.Extensions
{
    public static class CommandExtensions
    {
        public static IEnumerable<string> NonEmptyOut(this ICommand command)
        {
            return command.Out
                .Where(l => !string.IsNullOrWhiteSpace(l));
        }
    }
}
