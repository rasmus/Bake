using System;

namespace Bake.Commands
{
    public class ArgumentAttribute : Attribute
    {
        public string Description { get; }

        public ArgumentAttribute(
            string description)
        {
            Description = description;
        }
    }
}
