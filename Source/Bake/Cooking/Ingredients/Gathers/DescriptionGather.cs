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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bake.ValueObjects;

namespace Bake.Cooking.Ingredients.Gathers
{
    public class DescriptionGather : IGather
    {
        private static readonly IReadOnlySet<string> PossibleReadMeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "readme.md",
            "read-me.md",
        };

        public async Task GatherAsync(
            ValueObjects.Ingredients ingredients,
            CancellationToken cancellationToken)
        {
            var readMeFilePath = await Task.Run(() => GetPath(ingredients.WorkingDirectory), cancellationToken);
            if (string.IsNullOrEmpty(readMeFilePath))
            {
                ingredients.FailDescription();
                return;
            }

            var content = await File.ReadAllTextAsync(
                readMeFilePath,
                cancellationToken);

            ingredients.Description = new Description(content);
        }

        private static string? GetPath(string directoryPath)
        {
            while (true)
            {
                var readMeFilePath = Directory
                    .GetFiles(directoryPath)
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
