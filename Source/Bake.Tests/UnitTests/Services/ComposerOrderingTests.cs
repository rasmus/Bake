// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
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

using AutoFixture.Kernel;
using Bake.Cooking;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class ComposerOrderingTests : TestFor<ComposerOrdering>
    {
        private static readonly IReadOnlyCollection<ArtifactType> EmptyArtifactTypes = new ArtifactType[] { };

        [Test]
        public void BasicOrdering()
        {
            // Arrange
            var composers = new[]
                {
                    DummyProducer("E", ArtifactType.Executable),
                    Dummy("R", ArtifactType.Executable, ArtifactType.Release),
                    DummyProducer("E", ArtifactType.Executable),
                };

            // Act + Assert
            Test(composers, "E", "E", "R");
        }

        [Test]
        public void Current()
        {
            // Arrange
            var composers = typeof(Program).Assembly
                .Types()
                .Where(t => t.IsAssignableTo(typeof(IComposer)) && !t.IsAbstract)
                .Select(t => new SpecimenContext(Fixture).Resolve(t))
                .Cast<IComposer>()
                .ToList();

            // Act
            IReadOnlyCollection<IComposer>? ordered = null;
            Assert.DoesNotThrow(() => ordered = Sut.Order(composers));
            ordered.Should().NotBeNull();
            ordered.Should().HaveCount(composers.Count);
        }

        private void Test(
            IReadOnlyCollection<IComposer> composers,
            params string[] expectedOrder)
        {
            composers = Sut.Order(composers);
            var names = GetNames(composers);
            names.Should().BeEquivalentTo(
                expectedOrder,
                o => o.WithStrictOrdering(),
                $"{string.Join(",", names)} != {string.Join(",", expectedOrder)}");
        }

        private static IReadOnlyCollection<string> GetNames(IEnumerable<IComposer> composers)
        {
            return composers
                .Cast<DummyComposer>()
                .Select(d => d.Name)
                .ToList();
        }

        private IReadOnlyCollection<ArtifactType> A(params ArtifactType[] artifactTypes) => artifactTypes;

        private static IComposer DummyProducer(string name, params ArtifactType[] artifactTypes) => new DummyComposer(
            name,
            EmptyArtifactTypes,
            artifactTypes);
        
        private static IComposer DummyConsumer(string name, params ArtifactType[] artifactTypes) => new DummyComposer(
            name,
            artifactTypes,
            EmptyArtifactTypes);

        private static IComposer Dummy(string name, ArtifactType consume, ArtifactType produce) => new DummyComposer(
            name,
            new[] { consume },
            new[] { produce });

        private class DummyComposer : IComposer
        {
            private static readonly Task<IReadOnlyCollection<Recipe>> EmptyRecipes = Task.FromResult<IReadOnlyCollection<Recipe>>(new Recipe[] { });

            public string Name { get; }
            public IReadOnlyCollection<ArtifactType> Produces { get; }
            public IReadOnlyCollection<ArtifactType> Consumes { get; }

            public DummyComposer(string name,
                IReadOnlyCollection<ArtifactType> consumes,
                IReadOnlyCollection<ArtifactType> produces)
            {
                Name = name;
                Produces = produces;
                Consumes = consumes;
            }

            public Task<IReadOnlyCollection<Recipe>> ComposeAsync(
                IContext context,
                CancellationToken cancellationToken)
            {
                return EmptyRecipes;
            }
        }
    }
}
