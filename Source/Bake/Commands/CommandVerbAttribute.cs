using System;

namespace Bake.Commands
{
    public class CommandVerbAttribute : Attribute
    {
        public string Name { get; }

        public CommandVerbAttribute(
            string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }
    }
}
