using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.ValueObjects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bake.Cooking
{
    public class Editor : IEditor
    {
        private readonly IReadOnlyCollection<IComposer> _composers;

        public Editor(
            IEnumerable<IComposer> composers)
        {
            _composers = composers.ToList();
        }

        public async Task<Book> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            var tasks = _composers
                .Select(c => c.ComposeAsync(context, cancellationToken))
                .ToList();

            var recipes = (await Task.WhenAll(tasks))
                .SelectMany(c => c)
                .ToList();

            var book = new Book(
                context.Metadata,
                recipes);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new SemVerYamlTypeConverter())
                .Build();

            var yaml = serializer.Serialize(book);

            Console.WriteLine(yaml);

            return book;
        }
    }
}
