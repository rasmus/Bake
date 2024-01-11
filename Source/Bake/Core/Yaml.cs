// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Globalization;
using System.Reflection;
using Bake.ValueObjects;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Destinations;
using Bake.ValueObjects.Recipes;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.Utilities;

namespace Bake.Core
{
    public class Yaml : IYaml
    {
        private static readonly HashSet<Type> TypesWithTags = new()
        {
                typeof(Artifact),
                typeof(FileArtifact),
                typeof(Destination),
                typeof(Recipe),
            };

        private static readonly ISerializer Serializer;
        private static readonly IDeserializer Deserializer;

        static Yaml()
        {
            var recipeTypes = typeof(Yaml).Assembly
                .GetTypes()
                .Where(t => 
                    !t.IsAbstract && 
                    t.BaseType != null && 
                    TypesWithTags.Contains(t.BaseType))
                .Select(t =>
                {
                    var yamlTag = t.GetCustomAttributes().OfType<IYamlTag>().Single();
                    return new
                    {
                        tag = $"!{yamlTag.Name}",
                        type = t
                    };
                })
                .ToArray();

            Deserializer = recipeTypes.Aggregate(
                new DeserializerBuilder(),
                (b, a) => b.WithTagMapping(a.tag, a.type))
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new DateTimeOffsetTypeConverter())
                .WithTypeConverter(new SemVerYamlTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();
            Serializer = recipeTypes.Aggregate(
                new SerializerBuilder(),
                (b, a) => b.WithTagMapping(a.tag, a.type))
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new SemVerYamlTypeConverter())
                .WithTypeConverter(new DateTimeOffsetTypeConverter())
                .DisableAliases()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .Build();
        }

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
                () => (T) Deserializer.Deserialize(yaml, typeof(T))!,
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public class QuoteSurroundingEventEmitter : ChainedEventEmitter
        {
            public QuoteSurroundingEventEmitter(
                IEventEmitter nextEmitter)
                : base(nextEmitter)
            {
            }

            public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
            {
                eventInfo.Style = ScalarStyle.DoubleQuoted;

                base.Emit(eventInfo, emitter);
            }
        }

        private class DateTimeOffsetTypeConverter : IYamlTypeConverter
        {
            private static readonly IValueDeserializer ValueDeserializer = new DeserializerBuilder()
                .BuildValueDeserializer();

            private static readonly IValueSerializer ValueSerializer = new SerializerBuilder()
                .WithEventEmitter(n => new QuoteSurroundingEventEmitter(n))
                .BuildValueSerializer();

            public bool Accepts(Type type) => typeof(DateTimeOffset) == type;

            public object? ReadYaml(IParser parser, Type type)
            {
                var value = (string)ValueDeserializer.DeserializeValue(parser, typeof(string), new SerializerState(), ValueDeserializer)!;
                if (!DateTimeOffset.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffset))
                {
                    throw new FormatException($"'{value}' is not a valid DateTimeOffset");
                }

                return dateTimeOffset;
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                if (value == null)
                {
                    ValueSerializer.SerializeValue(emitter, null, typeof(string));
                }
                else
                {
                    var dateTimeOffset = (DateTimeOffset)value;
                    ValueSerializer.SerializeValue(emitter, dateTimeOffset.ToString("O"), typeof(string));
                }
            }
        }

        private class SemVerYamlTypeConverter : IYamlTypeConverter
        {
            private static readonly IValueDeserializer ValueDeserializer = new DeserializerBuilder()
                .BuildValueDeserializer();

            private static readonly IValueSerializer ValueSerializer = new SerializerBuilder()
                .WithEventEmitter(n => new QuoteSurroundingEventEmitter(n))
                .BuildValueSerializer();

            public bool Accepts(Type type) => typeof(SemVer) == type;

            public object? ReadYaml(IParser parser, Type type)
            {
                var value = (string) ValueDeserializer.DeserializeValue(parser, typeof(string), new SerializerState(), ValueDeserializer)!;
                if (!SemVer.TryParse(value, out var semVer))
                {
                    throw new FormatException($"'{value}' is not a valid SemVer");
                }

                return semVer;
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var semVer = (SemVer) value!;
                ValueSerializer.SerializeValue(emitter, semVer?.ToString(), typeof(string));
            }
        }
    }
}
