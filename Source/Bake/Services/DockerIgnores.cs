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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace Bake.Services
{
    public class DockerIgnores : IDockerIgnores
    {
        private static readonly IReadOnlyCollection<string> Ignores = new[]
        {
            "# Bake",
            "bake.yaml",
            ".bake-ignore",

            "# Credentials",
            "**/.aws",

            "# Repository stuff",
            "**/*.md",
            "**/LICENSE",

            "# Git",
            "**/.git*",
            "**/.gitignore",

            "# GitHub",
            "**/.github",

            "# Python",
            "**/env",
            "**/*.pyc",
            "**/.cache",
            "**/.coverage",
            "**/.pytest_cache",

            "# .NET",
            "**/obj",
            "**/*.*proj.user",

            "# Java",
            "**/.classpath",

            "# Docker files",
            "**/docker-compose*.yml",
            "**/.dockerignore",
            "**/Dockerfile*",

            "# Mac OS X",
            "**/.DS_Store",

            "# NodeJS",
            "**/node_modules",
            "**/.bundleignore",
            "**/.bundle",
            "**/.npmrc",
            "**/.env",
            "**/dist",

            "# Editors",
            "**/.vscode",
            "**/.idea",
            "**/.vs",
            "**/.editorconfig",

            "# Specific files",
            "**/Makefile",

            "# Build systems",
            "**/.gitlab-ci.yml",
            "**/.travis.yml",
            "**/.drone.yml",

            "# Helm",
            "**/charts",

            "# Others",
            "**/.env",
            "**/*~",
            "**/*.log",
            "**/.settings",
            "**/.toolstarget",
            "**/tmp",
            "**/.rspec",
        };

        private const string FileName = ".dockerignore";

        private readonly ILogger<DockerIgnores> _logger;

        public DockerIgnores(
            ILogger<DockerIgnores> logger)
        {
            _logger = logger;
        }

        public async Task WriteAsync(
            string directoryPath,
            CancellationToken cancellationToken)
        {
            var dockerIgnoreFilePath = Path.Join(directoryPath, FileName);
            if (File.Exists(dockerIgnoreFilePath))
            {
                _logger.LogInformation(
                    "There is already a Docker ignore file located at {FilePath}, skipping writing one",
                    dockerIgnoreFilePath);
                return;
            }

            var content = string.Join(Environment.NewLine, Ignores);
            await File.WriteAllTextAsync(
                dockerIgnoreFilePath,
                content,
                cancellationToken);
        }
    }
}
