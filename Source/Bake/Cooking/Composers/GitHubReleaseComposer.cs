// MIT License
// 
// Copyright (c) 2021-2025 Rasmus Mikkelsen
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

using Bake.Exceptions;
using Bake.Services;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Destinations;
using Bake.ValueObjects.Recipes;
using Bake.ValueObjects.Recipes.GitHub;

namespace Bake.Cooking.Composers
{
    public class GitHubReleaseComposer : Composer
    {
        private readonly IConventionInterpreter _conventionInterpreter;

        public override IReadOnlyCollection<ArtifactType> Consumes { get; } = [ArtifactType.Release];
        public override IReadOnlyCollection<ArtifactType> Produces { get; } = [ArtifactType.GitHubRelease];

        public GitHubReleaseComposer(
            IConventionInterpreter conventionInterpreter)
        {
            _conventionInterpreter = conventionInterpreter;
        }

        public override Task<IReadOnlyCollection<Recipe>> ComposeAsync(
            IContext context,
            CancellationToken cancellationToken)
        {
            if (context.Ingredients.GitHub == null ||
                context.Ingredients.Git == null)
            {
                throw new BuildFailedException("Missing git or GitHub information!");
            }

            if (!_conventionInterpreter.ShouldArtifactsBePublished(context.Ingredients.Convention))
            {
                return Task.FromResult(EmptyRecipes);
            }

            var gitHubDestination = context.Ingredients.Destinations
                .OfType<GitHubReleaseDestination>()
                .SingleOrDefault();
            if (gitHubDestination == null)
            {
                return Task.FromResult(EmptyRecipes);
            }

            var release = context.GetArtifacts<ReleaseArtifact>().SingleOrDefault();
            if (release == null)
            {
                return Task.FromResult(EmptyRecipes);
            }

            return Task.FromResult<IReadOnlyCollection<Recipe>>(
            [
                new GitHubReleaseRecipe(
                    release.Text,
                    context.Ingredients.GitHub,
                    context.Ingredients.Git.Sha,
                    release.Files)
            ]);
        }
    }
}
