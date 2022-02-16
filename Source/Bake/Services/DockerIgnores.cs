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
            "# Repository stuff",
            "**/*.md",

            "# Git",
            "**/.git*",

            "# Python",
            "**/env",
            "**/*.pyc",
            "**/.cache",
            "**/.coverage",
            "**/.pytest_cache",

            "# Docker files",
            "**/docker-compose*.yml",
            "**/.dockerignore",
            "**/Dockerfile*",

            "# Mac OS X",
            "**/.DS_Store",

            "# NodeJS",
            "**/node_modules",

            "# Code editors",
            "**/.vscode",
            "**/.idea",

            "# Specific files",
            "**/Makefile",

            "# Build systems",
            "**/.gitlab-ci.yml",
            "**/.travis.yml",

            "# Others",
            "**/.env",
            "**/*~",
            "**/*.log",
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
