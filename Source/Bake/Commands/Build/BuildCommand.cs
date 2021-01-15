using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking;
using Bake.Core;

namespace Bake.Commands.Build
{
    [CommandVerb("build")]
    public class BuildCommand : ICommand
    {
        private readonly IEditor _editor;
        private readonly IKitchen _kitchen;

        public BuildCommand(
            IEditor editor,
            IKitchen kitchen)
        {
            _editor = editor;
            _kitchen = kitchen;
        }

        public async Task<int> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var content = new Context(
                SemVer.With(0, 1),
                Directory.GetCurrentDirectory());

            var book = await _editor.ComposeAsync(
                content,
                cancellationToken);

            await _kitchen.CookAsync(
                content,
                book,
                cancellationToken);

            return 0;
        }
    }
}
