using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking;
using Bake.Core;
using Bake.ValueObjects;

namespace Bake.Commands.Run
{
    [CommandVerb("run")]
    public class RunCommand : ICommand
    {
        private readonly IEditor _editor;
        private readonly IKitchen _kitchen;

        public RunCommand(
            IEditor editor,
            IKitchen kitchen)
        {
            _editor = editor;
            _kitchen = kitchen;
        }

        public async Task<int> ExecuteAsync(
            SemVer buildVersion,
            int retry,
            CancellationToken cancellationToken)
        {
            var content = new Context(
                new Metadata()
                    .Set(SemVer.With(0, 1)),
                Directory.GetCurrentDirectory());

            var book = await _editor.ComposeAsync(
                content,
                cancellationToken);

            var success = await _kitchen.CookAsync(
                content,
                book,
                cancellationToken);

            return success ? 0 : -1;
        }
    }
}
