using System;

namespace Bake.Commands
{
    public class ArgumentAttribute : Attribute
    {
        public string Description { get; }
        public string DefaultValue { get; }

        public ArgumentAttribute(
            string description,
            string defaultValue = null)
        {
            Description = description;
            DefaultValue = defaultValue;
        }
    }
}
