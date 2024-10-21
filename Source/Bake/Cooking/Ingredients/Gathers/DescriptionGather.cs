// MIT License
// 
// Copyright (c) 2021-2024 Rasmus Mikkelsen
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

using Bake.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class DescriptionGather : IGather
    {
        private readonly ILogger<DescriptionGather> _logger;

        private static readonly IReadOnlySet<string> PossibleReadMeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "readme",
            "readme.md",
            "read-me",
            "read-me.md",
        };

        public DescriptionGather(
            ILogger<DescriptionGather> logger)
        {
            _logger = logger;
        }

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            try
            {
                await InternalGatherAsync(ingredients, cancellationToken);
            }
            catch (Exception e)
            {
                ingredients.FailDescription();
                _logger.LogError(e, "Failed to gather description information");
            }
        }

        private async Task InternalGatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Searching for a know file containing project description, starting from directory {WorkingDirectory}: {Files}",
                ingredients.WorkingDirectory,
                PossibleReadMeNames.ToArray());

            var readMeFilePath = await Task.Run(() => GetPath(ingredients.WorkingDirectory), cancellationToken);
            if (string.IsNullOrEmpty(readMeFilePath))
            {
                ingredients.FailDescription();
                _logger.LogInformation("Did not find any files containing a project description. Consider adding a README.md to the project root");
                return;
            }

            var content = await File.ReadAllTextAsync(
                readMeFilePath,
                cancellationToken);

            ingredients.Description = new Description(content);
        }

        private string? GetPath(string directoryPath)
        {
            while (true)
            {
                _logger.LogInformation("Searching for a suitable file in {DirectoryPath}", directoryPath);

                var readMeFilePath = Directory.GetFiles(directoryPath)
                    .FirstOrDefault(p => PossibleReadMeNames.Contains(Path.GetFileName(p)));
                if (!string.IsNullOrEmpty(readMeFilePath))
                {
                    return readMeFilePath;
                }

                var parent = Directory.GetParent(directoryPath);
                if (parent == null)
                {
                    return null;
                }

                directoryPath = parent.FullName;
            }
        }
    }
}
