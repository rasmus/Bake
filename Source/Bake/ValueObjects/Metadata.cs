using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Bake.Core;
using Bake.Extensions;

namespace Bake.ValueObjects
{
    public class Metadata : Dictionary<string, object>
    {
        private static readonly ConcurrentDictionary<Type, string> TypeMap = new ConcurrentDictionary<Type, string>();

        private static readonly IReadOnlyDictionary<string, object> Defaults = new object[]
            {
                SemVer.With(0, 1)
            }
            .ToDictionary(
                o => GetName(o.GetType()),
                o => o);

        public Metadata Set<T>(T obj)
        {
            this[GetName(typeof(T))] = obj;
            return this;
        }

        public T Get<T>()
        {
            var name = GetName(typeof(T));
            if (TryGetValue(name, out var o))
            {
                return (T) o;
            }

            if (Defaults.TryGetValue(name, out var d))
            {
                return (T) d;
            }

            throw new ArgumentOutOfRangeException($"No value for {name}");
        }

        private static string GetName(Type type)
        {
            return TypeMap.GetOrAdd(
                type,
                t =>
                {
                    var name = t.PrettyPrint();
                    return char.IsLower(name[0])
                        ? name
                        : $"{char.ToLower(name[0])}{name.Substring(1)}";
                });
        }
    }
}
