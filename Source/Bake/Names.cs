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
using System.Collections.Generic;
using System.Linq;
using Bake.ValueObjects.Artifacts;

namespace Bake
{
    public static class Names
    {
        /// <summary>
        /// Used for
        /// - YAML tags
        /// </summary>
        public static class Destinations
        {
            public const string ContainerRegistry = "container-registry";
            public const string NuGetRegistry = "nuget-registry";
            public const string Dynamic = "dynamic";
        }

        public static class DynamicDestinations
        {
            public const string GitHub = "github";
        }

        /// <summary>
        /// Used for
        /// - YAML tags
        /// </summary>
        public static class Artifacts
        {
            public const string FileArtifact = "file-artifact";
            public const string DirectoryArtifact = "directory-artifact";
        }

        public static class ArtifactTypes
        {
            public const string Container = "container";
            public const string Dockerfile = "dockerfile";
            public const string DotNetPublishedDirectory = "dotnet-publish-directory";
            public const string NuGet = "nuget";
            public const string ToolLinux = "tool-linux";
            public const string ToolWindows = "tool-windows";

            private static readonly IReadOnlyDictionary<ArtifactType, string> TypeToName = new Dictionary<ArtifactType, string>
                {
                    [ArtifactType.Container] = Container,
                    [ArtifactType.Dockerfile] = Dockerfile,
                    [ArtifactType.DotNetPublishedDirectory] = DotNetPublishedDirectory,
                    [ArtifactType.NuGet] = NuGet,
                    [ArtifactType.ToolLinux] = ToolLinux,
                    [ArtifactType.ToolWindows] = ToolWindows,
                };

            private static readonly IReadOnlyDictionary<string, ArtifactType> NameToType = TypeToName
                .ToDictionary(kv => kv.Value, kv => kv.Key);

            public static bool TryGetType(string name, out ArtifactType artifactType)
            {
                return NameToType.TryGetValue(name, out artifactType);
            }

            public static string GetName(ArtifactType artifactType)
            {
                if (!Enum.IsDefined(typeof(ArtifactType), artifactType) || artifactType == ArtifactType.Invalid)
                {
                    throw new ArgumentOutOfRangeException(nameof(artifactType), artifactType, null);
                }

                if (!TypeToName.TryGetValue(artifactType, out var name))
                {
                    throw new NotImplementedException();
                }

                return name;
            }
        }

        /// <summary>
        /// Used for
        /// - YAML tags
        /// </summary>
        public static class Recipes
        {
            public static class Go
            {
                public const string Build = "go-build";
                public const string Test = "go-test";
            }

            public static class Docker
            {
                public const string Build = "docker-build";
                public const string Push = "docker-push";
            }

            public static class DotNet
            {
                public const string Build = "dotnet-build";
                public const string Clean = "dotnet-clean";
                public const string Pack = "dotnet-pack";
                public const string Restore = "dotnet-restore";
                public const string Test = "dotnet-test";
                public const string NuGetPush = "dotnet-nuget-push";
                public const string Publish = "dotnet-publish";
                public const string DockerFile = "dotnet-dockerfile";
            }
        }
    }
}
