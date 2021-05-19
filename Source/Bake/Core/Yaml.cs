// MIT License
// 
// Copyright (c) 2021 Rasmus Mikkelsen
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

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bake.Extensions;
using Bake.ValueObjects;
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
        private static readonly ISerializer Serializer;

        private static readonly IDeserializer Deserializer;

        static Yaml()
        {
            var recipeType = typeof(Recipe);
            var recipeTypes = typeof(Yaml).Assembly
                .GetTypes()
                .Where(t => recipeType.IsAssignableFrom(t) && t != recipeType)
                .Select(t =>
                {
                    if (!(t.GetCustomAttribute(typeof(RecipeAttribute)) is RecipeAttribute attribute))
                    {
                        throw new ArgumentException($"'{t.PrettyPrint()}' does not have the {typeof(RecipeAttribute).PrettyPrint()} attribute");
                    }

                    return new
                    {
                        tag = $"!{attribute.Name}",
                        type = t
                    };
                })
                .ToArray();

            Deserializer = recipeTypes.Aggregate(
                new DeserializerBuilder(),
                (b, a) => b.WithTagMapping(a.tag, a.type))
                .WithTagMapping("!file-artifact", typeof(FileArtifact))
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new SemVerYamlTypeConverter())
                .Build();
            Serializer = recipeTypes.Aggregate(
                new SerializerBuilder(),
                (b, a) => b.WithTagMapping(a.tag, a.type))
                .WithTagMapping("!file-artifact", typeof(FileArtifact))
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new SemVerYamlTypeConverter())
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
                () => (T) Deserializer.Deserialize(yaml, typeof(T)),
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
                var value = (string) ValueDeserializer.DeserializeValue(parser, typeof(string), new SerializerState(), ValueDeserializer);
                if (!SemVer.TryParse(value, out var semVer))
                {
                    throw new FormatException($"'{value}' is not a valid SemVer");
                }

                return semVer;
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                var semVer = (SemVer) value;
                ValueSerializer.SerializeValue(emitter, semVer?.ToString(), typeof(string));
            }
        }
    }
}