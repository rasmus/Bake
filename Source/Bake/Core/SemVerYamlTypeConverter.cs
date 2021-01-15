using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Bake.Core
{
    public class SemVerYamlTypeConverter : IYamlTypeConverter
    {
        private static readonly Type SemVerType = typeof(SemVer);

        public bool Accepts(Type type)
        {
            return type == SemVerType;
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            var str = parser.Consume<Scalar>().Value;
            return SemVer.Parse(str);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var semVer = (SemVer) value;
            emitter.Emit(new Scalar(null, null, semVer.ToString(), ScalarStyle.DoubleQuoted, true, false));
        }
    }
}
