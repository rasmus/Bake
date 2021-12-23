using System;
using Bake.ValueObjects.Artifacts;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.Python
{
    [Recipe(Names.Recipes.Python.FlaskDockerfile)]
    public class PythonFlaskDockerfileRecipe : Recipe
    {
        [YamlMember]
        public string Directory { get; [Obsolete] set; }

        [Obsolete]
        public PythonFlaskDockerfileRecipe() { }

        public PythonFlaskDockerfileRecipe(
            string directory,
            params Artifact[] artifacts)
            : base(artifacts)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Directory = directory;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
