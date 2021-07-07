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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bake.Tests.Helpers
{
    public abstract class BakeTest : TestProject
    {
        private CancellationTokenSource _timeout;

        protected BakeTest(string projectName) : base(projectName)
        {
        }

        [SetUp]
        public void SetUpBakeTest()
        {
            _timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        }

        [TearDown]
        public void TearDownBakeTest()
        {
            _timeout.Dispose();
            _timeout = null;
        }

        protected Task<int> ExecuteAsync(
            params string[] args)
        {
            return ExecuteAsync(TestState.New(args));
        }

        protected Task<int> ExecuteAsync(
            TestState testState)
        {
            return Program.EntryAsync(
                testState.Arguments,
                testState.Overrides,
                CancellationToken.None);
        }
    }
}
