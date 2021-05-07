﻿using System.Threading;
using System.Threading.Tasks;
using Bake.Cooking.Composers;
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
        public async Task T()
        {
            // Act
            var recipes = await Sut.ComposeAsync(
                new Context(new Metadata(), WorkingDirectory), 
                CancellationToken.None);
        }

        protected override IServiceCollection Configure(IServiceCollection serviceCollection)
        {
            return base.Configure(serviceCollection)
                .AddTransient<ICsProjParser, CsProjParser>();
        }
    }
}
