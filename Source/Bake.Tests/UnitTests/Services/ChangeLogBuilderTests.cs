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
using Bake.Services;
using Bake.Tests.Helpers;
using Bake.ValueObjects;

namespace Bake.Tests.UnitTests.Services
{
    public class ChangeLogBuilderTests : TestFor<ChangeLogBuilder>
    {
        private static IReadOnlyCollection<PullRequest> Get()
        {
            return new PullRequest[]
            {
                new (236, "Bump NuGet.Packaging from 6.6.0 to 6.6.1"),
                new (235, "Post release fixes"),
                new (237, "Bump YamlDotNet from 13.1.0 to 13.1.1"),
                new (240, "Bump WireMock.Net from 1.5.28 to 1.5.29"),
                new (241, "Bump semver from 7.5.1 to 7.5.3 in /TestProjects/NodeJS.Service"),
                new (239, "Bump Octokit from 6.0.0 to 6.1.0"),
                new (242, "Bump WireMock.Net from 1.5.29 to 1.5.30"),
                new (243, "Bump Microsoft.NET.Test.Sdk from 17.6.2 to 17.6.3"),
                new (244, "Bump Octokit from 6.1.0 to 6.2.1"),
                new (246, "Bump fastify from 4.18.0 to 4.19.1 in /TestProjects/NodeJS.Service"),
                new (248, "Bump fastify from 4.19.1 to 4.19.2 in /TestProjects/NodeJS.Service"),
                new (247, "Bump Octokit from 6.2.1 to 7.0.0"),
                new (249, "Bump Octokit from 7.0.0 to 7.0.1"),
                new (250, "Bump WireMock.Net from 1.5.30 to 1.5.31"),
                new (252, "Bump github.com/labstack/echo/v4 from 4.10.2 to 4.11.1 in /TestProjects/GoLang.Service"),
                new (254, "Bump fastify from 4.19.2 to 4.20.0 in /TestProjects/NodeJS.Service"),
                new (255, "Bump Octokit from 7.0.1 to 7.1.0"),
                new (253, "Bump WireMock.Net from 1.5.31 to 1.5.32"),
                new (256, "Bump fastify from 4.20.0 to 4.21.0 in /TestProjects/NodeJS.Service"),
                new (259, "Bump Microsoft.NET.Test.Sdk from 17.6.3 to 17.7.0"),
                new (258, "Bump WireMock.Net from 1.5.32 to 1.5.34"),
                new (260, "Remove moq"),
                new (261, "Bump NuGet.Packaging from 6.6.1 to 6.7.0"),
                new (262, "Bump YamlDotNet from 13.1.1 to 13.2.0"),
                new (263, "Bump Microsoft.NET.Test.Sdk from 17.7.0 to 17.7.1"),
                new (264, "Bump WireMock.Net from 1.5.34 to 1.5.35"),
                new (265, "Bump flask from 2.3.2 to 2.3.3 in /TestProjects/Python3.Flask"),
                new (266, "Bump FluentAssertions from 6.11.0 to 6.12.0"),
                new (267, "Bump fastify from 4.21.0 to 4.22.0 in /TestProjects/NodeJS.Service"),
                new (269, "Bump YamlDotNet from 13.2.0 to 13.3.1"),
                new (268, "Bump McMaster.Extensions.CommandLineUtils from 4.0.2 to 4.1.0"),
                new (270, "Bump Microsoft.NET.Test.Sdk from 17.7.1 to 17.7.2"),
                new (271, "Bump fastify from 4.22.0 to 4.22.1 in /TestProjects/NodeJS.Service"),
                new (272, "Bump fastify from 4.22.1 to 4.22.2 in /TestProjects/NodeJS.Service"),
                new (275, "Bump fastify from 4.22.2 to 4.23.0 in /TestProjects/NodeJS.Service"),
                new (276, "Bump fastify from 4.23.0 to 4.23.1 in /TestProjects/NodeJS.Service"),
                new (277, "Bump fastify from 4.23.1 to 4.23.2 in /TestProjects/NodeJS.Service"),
                new (278, "Bump YamlDotNet from 13.3.1 to 13.4.0"),
                new (279, "Bump WireMock.Net from 1.5.35 to 1.5.36"),
                new (281, "Bump WireMock.Net from 1.5.36 to 1.5.37"),
                new (284, "Bump YamlDotNet from 13.4.0 to 13.5.1"),
                new (285, "Bump YamlDotNet from 13.5.1 to 13.5.2"),
                new (286, "Bump WireMock.Net from 1.5.37 to 1.5.39"),
                new (287, "Bump YamlDotNet from 13.5.2 to 13.7.0"),
                new (288, "Bump github.com/labstack/echo/v4 from 4.11.1 to 4.11.2 in /TestProjects/GoLang.Service"),
                new (289, "Bump fastify from 4.23.2 to 4.24.0 in /TestProjects/NodeJS.Service"),
                new (291, "Bump fastify from 4.24.0 to 4.24.2 in /TestProjects/NodeJS.Service"),
                new (293, "Bump fastify from 4.24.2 to 4.24.3 in /TestProjects/NodeJS.Service"),
                new (294, "Bump actions/setup-node from 3 to 4"),
                new (292, "Bump YamlDotNet from 13.7.0 to 13.7.1"),
                new (282, "Bump flask from 2.3.3 to 3.0.0 in /TestProjects/Python3.Flask"),
                new (296, "Bump LibGit2Sharp from 0.27.2 to 0.28.0"),
                new (297, "Bump NUnit from 3.13.3 to 3.14.0"),
                new (273, "Bump actions/checkout from 3 to 4"),
                new (298, "Bump github.com/labstack/echo/v4 from 4.11.2 to 4.11.3 in /TestProjects/GoLang.Service"),
                new (299, "Bump WireMock.Net from 1.5.39 to 1.5.40"),
                new (300, "Bump Microsoft.NET.Test.Sdk from 17.7.2 to 17.8.0"),
                new (308, "Update and test .NET 8"),
                new (309, "Bump NuGet.Packaging from 6.7.0 to 6.8.0"),
                new (311, "Bump actions/setup-dotnet from 3 to 4"),
                new (313, "Bump actions/setup-go from 4 to 5"),
                new (314, "Bump WireMock.Net from 1.5.40 to 1.5.42"),
                new (315, "Bump LibGit2Sharp from 0.28.0 to 0.29.0"),
                new (316, "Bump AutoFixture.AutoNSubstitute from 4.18.0 to 4.18.1"),
                new (318, "Bump WireMock.Net from 1.5.42 to 1.5.43"),
                new (312, "Bump actions/setup-python from 4 to 5"),
                new (319, "Don't print commands"),
            };
        }
    }
}
