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

using Bake.Cooking;
using Bake.Cooking.Cooks;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using Bake.ValueObjects.Recipes;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Bake.Tests.UnitTests.Cooking
{
    public class KitchenTests : TestIt
    {
        [Test]
        public async Task SimilarTaskNamesAreBundledInTimings()
        {
            // Arrange
            var sut = new Kitchen(
                Mock<Microsoft.Extensions.Logging.ILogger<Kitchen>>(),
                new ICook[]
                {
                    new DummyPackCook(),
                    new DummyTestCook(),
                });
            var context = Substitute.For<IContext>();
            var book = new Book(
                A<Bake.ValueObjects.Ingredients>(),
                new Recipe[]
                {
                    new DummyPackRecipe(),
                    new DummyPackRecipe(),
                    new DummyTestRecipe(),
                });
            var stringWriter = new StringWriter();
            var existingOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                // Act
                var success = await sut.CookAsync(
                    context,
                    book,
                    CancellationToken.None);

                // Assert
                success.ShouldBeTrue();
                var output = stringWriter.ToString();
                output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Count(l => l.Contains("dotnet-pack"))
                    .ShouldBe(1);
                output.ShouldContain("dotnet-pack x2");
                output.ShouldContain("dotnet-test");
            }
            finally
            {
                Console.SetOut(existingOut);
            }
        }

        [Recipe("dummy-pack")]
        private class DummyPackRecipe : Recipe
        {
        }

        [Recipe("dummy-test")]
        private class DummyTestRecipe : Recipe
        {
        }

        private class DummyPackCook : Cook<DummyPackRecipe>
        {
            protected override string GetName(DummyPackRecipe recipe)
            {
                return "dotnet-pack";
            }

            protected override Task<bool> CookAsync(
                IContext context,
                DummyPackRecipe recipe,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }

        private class DummyTestCook : Cook<DummyTestRecipe>
        {
            protected override string GetName(DummyTestRecipe recipe)
            {
                return "dotnet-test";
            }

            protected override Task<bool> CookAsync(
                IContext context,
                DummyTestRecipe recipe,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}
