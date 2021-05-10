using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bake.Core
{
    public class Yaml : IYaml
    {
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new SemVerYamlTypeConverter())
            .Build();

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new SemVerYamlTypeConverter())
            .Build();

        public async Task<string> SerializeAsync<T>(
            T obj,
            CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(
                () => Serializer.Serialize(obj),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public async Task<T> DeserializeAsync<T>(
            string yaml,
            CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(
                () => (T) Deserializer.Deserialize(yaml, typeof(T)),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
    }
}
