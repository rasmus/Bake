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
using System.Threading.Tasks;
using Bake.Core;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Ingredients
    {
        public static Ingredients New(
            SemVer version,
            string workingDirectory) => new Ingredients(
            version,
            new Credentials(),
            workingDirectory);

        [YamlMember(Alias = "version")]
        public SemVer Version { get; [Obsolete] set; }

        [YamlIgnore]
        public Credentials Credentials { get; [Obsolete] set; }

        [YamlMember(Alias = "workingDirectory")]
        public string WorkingDirectory { get; [Obsolete] set; }

        [YamlMember(Alias = "git")]
        public GitInformation Git
        {
            get => _git.Task.IsCompletedSuccessfully ? _git.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }
                _git.SetResult(value);
            }
        }

        [YamlMember(Alias = "releaseNotes")]
        public ReleaseNotes ReleaseNotes
        {
            get => _releaseNotes.Task.IsCompletedSuccessfully ? _releaseNotes.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }
                _releaseNotes .SetResult(value);
            }
        }

        [YamlIgnore]
        public Task<GitInformation> GitTask => _git.Task;

        [YamlIgnore]
        public Task<ReleaseNotes> ReleaseNotesTask => _releaseNotes.Task;

        private readonly TaskCompletionSource<GitInformation> _git = new TaskCompletionSource<GitInformation>();
        private readonly TaskCompletionSource<ReleaseNotes> _releaseNotes = new TaskCompletionSource<ReleaseNotes>();

        [Obsolete]
        public Ingredients() { }

        public Ingredients(
            SemVer version,
            Credentials credentials,
            string workingDirectory)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Version = version;
            Credentials = credentials;
            WorkingDirectory = workingDirectory;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
