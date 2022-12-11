// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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

using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Composers;
using Bake.Core;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Bake.Tests.IntegrationTests.ServiceTests
{
    public class DotNetComposerServiceTests : ServiceTest<DotNetComposer>
    {
        public DotNetComposerServiceTests() : base("NetCore.Console")
        {
        }

        [Test]
        public async Task TestIt()
        {
            // Arrange
            var ingredients = Ingredients.New(
                SemVer.With(1, 2, 3),
                WorkingDirectory);

            // Act
            var recipesTask = Sut.ComposeAsync(
                Context.New(ingredients), 
                CancellationToken.None);
            ingredients.FailOutstanding();

            // Arrange
            var _ = await recipesTask;
        }

        protected override IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return base.Configure(serviceCollection)
                .AddTransient<ICsProjParser, CsProjParser>()
                .AddTransient<IFileSystem, FileSystem>()
                .AddTransient<ICredentials, Credentials>()
                .AddTransient<IDefaults, Defaults>()
                .AddTransient<IDescriptionLimiter, DescriptionLimiter>()
                .AddSingleton(TestEnvironmentVariables.None)
                .AddTransient<IConventionInterpreter, ConventionInterpreter>()
                .AddTransient<IDotNetTfmParser, DotNetTfmParser>()
                .AddTransient<IDockerLabels, DockerLabels>()
                .AddTransient<IBakeProjectParser, BakeProjectParser>();
        }
    }
}
