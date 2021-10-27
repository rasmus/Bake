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
using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Ingredients.Gathers;
using Bake.Core;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable StringLiteralTypo

namespace Bake.Tests.UnitTests.Ingredients.Gathers
{
    public class GitHubGatherTests : TestFor<GitHubGather>
    {
        [SetUp]
        public void SetUp()
        {
            Inject<IDefaults>(new Defaults());
        }

        [TestCase("https://github.com/rasmus/Bake.git", "rasmus", "Bake")]
        [TestCase("https://github.com/rasmus/Bake", "rasmus", "Bake")]
        public async Task Verify(string url, string expectedOwner, string expectedRepository)
        {
            // Arrange
            var ingredients = new Bake.ValueObjects.Ingredients(
                SemVer.Random,
                string.Empty,
                Convention.Default);

            // Act
            var gather = Sut.GatherAsync(ingredients, CancellationToken.None);
            ingredients.Git = new GitInformation(
                "sha",
                new Uri(url));
            await gather;
            var gitHubInformation = await ingredients.GitHubTask;

            // Assert
            gitHubInformation.Owner.Should().Be(expectedOwner);
            gitHubInformation.Repository.Should().Be(expectedRepository);
        }
    }
}
