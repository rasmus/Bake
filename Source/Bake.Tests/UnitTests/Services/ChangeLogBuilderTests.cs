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

using System.Collections.Generic;
using System.Linq;
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Services
{
    public class ChangeLogBuilderTests : TestFor<ChangeLogBuilder>
    {
        [Test]
        public void Build()
        {
            // Arrange
            var pullRequests = Get();

            // Act
            var changes = Sut.Build(pullRequests);

            // Assert
            changes.Should().HaveCount(2);
            changes[ChangeType.Dependency].Should().HaveCount(21);
            changes[ChangeType.Dependency].Select(d => d.Text).Should().Contain("Bump flask 2.3.2 to 3.0.0 in /TestProjects/Python3.Flask (#265, #282, by @dependabot)");
        }

        private static IReadOnlyCollection<PullRequest> Get()
        {
            return new PullRequest[]
            {
                new (236, "Bump NuGet.Packaging from 6.6.0 to 6.6.1", new []{"dependabot"}),
                new (235, "Post release fixes", new []{"rasmus"}),
                new (237, "Bump YamlDotNet from 13.1.0 to 13.1.1", new []{"dependabot"}),
                new (240, "Bump WireMock.Net from 1.5.28 to 1.5.29", new []{"dependabot"}),
                new (241, "Bump semver from 7.5.1 to 7.5.3 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (239, "Bump Octokit from 6.0.0 to 6.1.0", new []{"dependabot"}),
                new (242, "Bump WireMock.Net from 1.5.29 to 1.5.30", new []{"dependabot"}),
                new (243, "Bump Microsoft.NET.Test.Sdk from 17.6.2 to 17.6.3", new []{"dependabot"}),
                new (244, "Bump Octokit from 6.1.0 to 6.2.1", new []{"dependabot"}),
                new (246, "Bump fastify from 4.18.0 to 4.19.1 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (248, "Bump fastify from 4.19.1 to 4.19.2 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (247, "Bump Octokit from 6.2.1 to 7.0.0", new []{"dependabot"}),
                new (249, "Bump Octokit from 7.0.0 to 7.0.1", new []{"dependabot"}),
                new (250, "Bump WireMock.Net from 1.5.30 to 1.5.31", new []{"dependabot"}),
                new (252, "Bump github.com/labstack/echo/v4 from 4.10.2 to 4.11.1 in /TestProjects/GoLang.Service", new []{"dependabot"}),
                new (254, "Bump fastify from 4.19.2 to 4.20.0 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (255, "Bump Octokit from 7.0.1 to 7.1.0", new []{"dependabot"}),
                new (253, "Bump WireMock.Net from 1.5.31 to 1.5.32", new []{"dependabot"}),
                new (256, "Bump fastify from 4.20.0 to 4.21.0 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (259, "Bump Microsoft.NET.Test.Sdk from 17.6.3 to 17.7.0", new []{"dependabot"}),
                new (258, "Bump WireMock.Net from 1.5.32 to 1.5.34", new []{"dependabot"}),
                new (260, "Remove moq", new []{"rasmus"}),
                new (261, "Bump NuGet.Packaging from 6.6.1 to 6.7.0", new []{"dependabot"}),
                new (262, "Bump YamlDotNet from 13.1.1 to 13.2.0", new []{"dependabot"}),
                new (263, "Bump Microsoft.NET.Test.Sdk from 17.7.0 to 17.7.1", new []{"dependabot"}),
                new (264, "Bump WireMock.Net from 1.5.34 to 1.5.35", new []{"dependabot"}),
                new (265, "Bump flask from 2.3.2 to 2.3.3 in /TestProjects/Python3.Flask", new []{"dependabot"}),
                new (266, "Bump FluentAssertions from 6.11.0 to 6.12.0", new []{"dependabot"}),
                new (267, "Bump fastify from 4.21.0 to 4.22.0 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (269, "Bump YamlDotNet from 13.2.0 to 13.3.1", new []{"dependabot"}),
                new (268, "Bump McMaster.Extensions.CommandLineUtils from 4.0.2 to 4.1.0", new []{"dependabot"}),
                new (270, "Bump Microsoft.NET.Test.Sdk from 17.7.1 to 17.7.2", new []{"dependabot"}),
                new (271, "Bump fastify from 4.22.0 to 4.22.1 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (272, "Bump fastify from 4.22.1 to 4.22.2 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (275, "Bump fastify from 4.22.2 to 4.23.0 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (276, "Bump fastify from 4.23.0 to 4.23.1 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (277, "Bump fastify from 4.23.1 to 4.23.2 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (278, "Bump YamlDotNet from 13.3.1 to 13.4.0", new []{"dependabot"}),
                new (279, "Bump WireMock.Net from 1.5.35 to 1.5.36", new []{"dependabot"}),
                new (281, "Bump WireMock.Net from 1.5.36 to 1.5.37", new []{"dependabot"}),
                new (284, "Bump YamlDotNet from 13.4.0 to 13.5.1", new []{"dependabot"}),
                new (285, "Bump YamlDotNet from 13.5.1 to 13.5.2", new []{"dependabot"}),
                new (286, "Bump WireMock.Net from 1.5.37 to 1.5.39", new []{"dependabot"}),
                new (287, "Bump YamlDotNet from 13.5.2 to 13.7.0", new []{"dependabot"}),
                new (288, "Bump github.com/labstack/echo/v4 from 4.11.1 to 4.11.2 in /TestProjects/GoLang.Service", new []{"dependabot"}),
                new (289, "Bump fastify from 4.23.2 to 4.24.0 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (291, "Bump fastify from 4.24.0 to 4.24.2 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (293, "Bump fastify from 4.24.2 to 4.24.3 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (294, "Bump actions/setup-node from 3 to 4", new []{"dependabot"}),
                new (292, "Bump YamlDotNet from 13.7.0 to 13.7.1", new []{"dependabot"}),
                new (282, "Bump flask from 2.3.3 to 3.0.0 in /TestProjects/Python3.Flask", new []{"dependabot"}),
                new (296, "Bump LibGit2Sharp from 0.27.2 to 0.28.0", new []{"dependabot"}),
                new (297, "Bump NUnit from 3.13.3 to 3.14.0", new []{"dependabot"}),
                new (273, "Bump actions/checkout from 3 to 4", new []{"dependabot"}),
                new (298, "Bump github.com/labstack/echo/v4 from 4.11.2 to 4.11.3 in /TestProjects/GoLang.Service", new []{"dependabot"}),
                new (299, "Bump WireMock.Net from 1.5.39 to 1.5.40", new []{"dependabot"}),
                new (300, "Bump Microsoft.NET.Test.Sdk from 17.7.2 to 17.8.0", new []{"dependabot"}),
                new (308, "Update and test .NET 8", new []{"rasmus"}),
                new (309, "Bump NuGet.Packaging from 6.7.0 to 6.8.0", new []{"dependabot"}),
                new (311, "Bump actions/setup-dotnet from 3 to 4", new []{"dependabot"}),
                new (313, "Bump actions/setup-go from 4 to 5", new []{"dependabot"}),
                new (314, "Bump WireMock.Net from 1.5.40 to 1.5.42", new []{"dependabot"}),
                new (315, "Bump LibGit2Sharp from 0.28.0 to 0.29.0", new []{"dependabot"}),
                new (316, "Bump AutoFixture.AutoNSubstitute from 4.18.0 to 4.18.1", new []{"dependabot"}),
                new (318, "Bump WireMock.Net from 1.5.42 to 1.5.43", new []{"dependabot"}),
                new (312, "Bump actions/setup-python from 4 to 5", new []{"dependabot"}),
                new (319, "Don't print commands", new []{"rasmus"}),
                new (321, "Bump fastify from 4.24.3 to 4.25.0 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (323, "Bump WireMock.Net from 1.5.43 to 1.5.44", new []{"dependabot"}),
                new (325, "Bump fastify from 4.25.0 to 4.25.1 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (326, "Bump golang.org/x/crypto from 0.14.0 to 0.17.0 in /TestProjects/GoLang.Service", new []{"dependabot"}),
                new (322, "Bump github/codeql-action from 2 to 3", new []{"dependabot"}),
                new (317, "Bump NUnit from 3.14.0 to 4.0.1", new []{"dependabot"}),
                new (327, "Bump github.com/labstack/echo/v4 from 4.11.3 to 4.11.4 in /TestProjects/GoLang.Service", new []{"dependabot"}),
                new (328, "Bump WireMock.Net from 1.5.44 to 1.5.45", new []{"dependabot"}),
                new (329, "Bump fastify from 4.25.1 to 4.25.2 in /TestProjects/NodeJS.Service", new []{"dependabot"}),
                new (330, "Bump WireMock.Net from 1.5.45 to 1.5.46", new []{"dependabot"}),
            };
        }
    }
}
