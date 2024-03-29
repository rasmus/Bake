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

using System;
using System.Collections.Generic;
using System.Linq;
using Bake.Cooking;
using Bake.Extensions;
using Microsoft.Extensions.Logging;

namespace Bake.Services
{
    public class ComposerOrdering : IComposerOrdering
    {
        private readonly ILogger<ComposerOrdering> _logger;

        public ComposerOrdering(
            ILogger<ComposerOrdering> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<IComposer> Order(IEnumerable<IComposer> composers)
        {
            var unordered = composers
                .OrderBy(c => c.GetType().PrettyPrint())
                .ToList();
            var ordered = new List<IComposer>();

            while (unordered.Any())
            {
                var satisfied = unordered
                    .FirstOrDefault(c => 
                        c.Consumes.All(a => ordered.Any(o => o.Produces.Contains(a))) &&
                        c.Consumes.All(a => unordered.All(o => !o.Produces.Contains(a)))
                        );
                if (satisfied == null)
                {
                    throw new Exception("Could not find a consumer to pick for the next in line");
                }

                ordered.Add(satisfied);
                unordered.Remove(satisfied);
            }

            _logger.LogInformation(
                "Ordering composers like this: {Composers}",
                ordered.Select(c => c.GetType().PrettyPrint()));

            return ordered;
        }
    }
}
