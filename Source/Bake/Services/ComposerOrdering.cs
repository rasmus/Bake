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
using Bake.Cooking;

namespace Bake.Services
{
    public class ComposerOrdering : IComposerOrdering
    {
        public IReadOnlyCollection<IComposer> Order(IEnumerable<IComposer> composers)
        {
            var unordered = composers.ToList();
            var ordered = new List<IComposer>();

            while (unordered.Any())
            {
                var satisfied = unordered
                    .Where(c => c.Consumes.All(a => ordered.Any(o => o.Produces.Contains(a))))
                    .ToList();

                if (!satisfied.Any())
                {
                    throw new Exception("Cannot order any more");
                }

                ordered.AddRange(satisfied);
                foreach (var composer in satisfied)
                {
                    unordered.Remove(composer);
                }
            }

            return ordered;
        }
    }
}
