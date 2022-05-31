using System;
using System.Collections.Generic;
using Bake.ValueObjects.Artifacts;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects.Recipes.NodeJS
{
    [Recipe(Names.Recipes.NodeJS.Dockerfile)]
    public class NodeJSDockerfileRecipe : Recipe
    {
        [YamlMember]
        public string WorkingDirectory { get; [Obsolete] set; }

        [YamlMember]
        public Dictionary<string, string> Labels { get; [Obsolete] set; }

        [Obsolete]
        public NodeJSDockerfileRecipe() { }

        public NodeJSDockerfileRecipe(
            string workingDirectory,
            Dictionary<string, string> labels,
            params Artifact[] artifacts)
            : base(artifacts)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            WorkingDirectory = workingDirectory;
            Labels = labels;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
