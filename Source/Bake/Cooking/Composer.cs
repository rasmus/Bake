using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;

namespace Bake.Cooking
{
    public abstract class Composer : IComposer
    {
        private static readonly IReadOnlyCollection<ArtifactType> EmptyArtifactTypes = new ArtifactType[] { };

        public virtual IReadOnlyCollection<ArtifactType> Produces { get; } = EmptyArtifactTypes;
        public virtual IReadOnlyCollection<ArtifactType> Consumes { get; } = EmptyArtifactTypes;

        public abstract Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken);
    }
}
