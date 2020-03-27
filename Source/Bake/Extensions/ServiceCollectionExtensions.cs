using System;
using System.Collections.Generic;
using Bake.Cookbooks.Recipes;
using Bake.Cookbooks.Recipes.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Bake.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ScanByConvention<T>(
            this IServiceCollection serviceCollection,
            IEnumerable<Type> singletons)
        {
            var hashSet = new HashSet<Type>(singletons);

            serviceCollection.Scan(s =>
                {
                    s
                        .FromAssemblyOf<Program>()
                        .AddClasses(f => f.Where(t => !hashSet.Contains(t)))
                        .AsMatchingInterface()
                        .WithTransientLifetime();
                    s
                        .FromAssemblyOf<Program>()
                        .AddClasses(f => f.AssignableTo<IRecipe>())
                        .As<IRecipe>()
                        .WithTransientLifetime();
                    s
                        .FromAssemblyOf<Program>()
                        .AddClasses(f => f.AssignableTo<IStep>())
                        .As<IStep>()
                        .WithTransientLifetime();
                    s
                        .AddTypes(hashSet)
                        .AsMatchingInterface()
                        .WithSingletonLifetime();
                });

            return serviceCollection;
        }
    }
}
