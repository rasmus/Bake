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

using Bake.Core;
using Bake.ValueObjects.Destinations;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Ingredients
    {
        public static Ingredients New(
            SemVer version,
            string workingDirectory,
            IReadOnlyCollection<Platform>? targetPlatforms = null,
            Convention convention = Convention.Default,
            bool pushContainerLatestTag = false,
            bool signArtifacts = false) => new(
                version,
                workingDirectory,
                targetPlatforms != null && targetPlatforms.Any()
                    ? targetPlatforms.ToArray()
                    : Platform.Defaults,
                convention,
                pushContainerLatestTag,
                signArtifacts);

        [YamlMember]
        public SemVer Version { get; [Obsolete] set; }

        [YamlMember]
        public string WorkingDirectory { get; [Obsolete] set; }

        [YamlMember]
        public Convention Convention { get; [Obsolete] set; }

        [YamlMember]
        public bool PushContainerLatestTag { get; [Obsolete] set; }

        [YamlMember]
        public bool SignArtifacts { get; [Obsolete] set; }

        [YamlMember]
        public Platform[] Platforms { get; [Obsolete] set; }

        [YamlMember]
        public List<Destination> Destinations { get; [Obsolete] set; } = new();

        [YamlMember]
        public ChangeLog? Changelog
        {
            get => _changelog.Task.IsCompletedSuccessfully ? _changelog.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }
                _changelog.SetResult(value);
            }
        }

        [YamlMember]
        public GitInformation? Git
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

        [YamlMember]
        public GitHubInformation? GitHub
        {
            get => _gitHub.Task.IsCompletedSuccessfully ? _gitHub.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }
                _gitHub.SetResult(value);
            }
        }

        [YamlMember]
        public PullRequestInformation? PullRequest
        {
            get => _pullRequest.Task.IsCompletedSuccessfully ? _pullRequest.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }
                _pullRequest.SetResult(value);
            }
        }

        [YamlMember]
        public Description? Description
        {
            get => _description.Task.IsCompletedSuccessfully ? _description.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }
                _description.SetResult(value);
            }
        }

        [YamlMember]
        public ReleaseNotes? ReleaseNotes
        {
            get => _releaseNotes.Task.IsCompletedSuccessfully ? _releaseNotes.Task.Result : null;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(value.Version.Meta) &&
                    string.IsNullOrEmpty(Version.Meta))
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    Version = Version.WithMeta(value.Version.Meta);
#pragma warning restore CS0612 // Type or member is obsolete
                }

                _releaseNotes.SetResult(value);
            }
        }

        [YamlIgnore]
        public Task<GitInformation> GitTask => _git.Task;

        [YamlIgnore]
        public Task<Description> DescriptionTask => _description.Task;

        [YamlIgnore]
        public Task<ReleaseNotes> ReleaseNotesTask => _releaseNotes.Task;

        [YamlIgnore]
        public Task<GitHubInformation> GitHubTask => _gitHub.Task;

        [YamlIgnore]
		public Task<ChangeLog> ChangelogTask => _changelog.Task;
		
        [YamlIgnore]
        public Task<PullRequestInformation> PullRequestTask => _pullRequest.Task;
		
        private readonly TaskCompletionSource<GitInformation> _git = new();
        private readonly TaskCompletionSource<ReleaseNotes> _releaseNotes = new();
        private readonly TaskCompletionSource<GitHubInformation> _gitHub = new();
        private readonly TaskCompletionSource<ChangeLog> _changelog = new();
        private readonly TaskCompletionSource<Description> _description = new();
        private readonly TaskCompletionSource<PullRequestInformation> _pullRequest = new();

        [Obsolete]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Ingredients() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Ingredients(
            SemVer version,
            string workingDirectory,
            Platform[] platforms,
            Convention convention,
            bool pushContainerLatestTag,
            bool signArtifacts)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Version = version;
            WorkingDirectory = workingDirectory;
            Platforms = platforms;
            Convention = convention;
            PushContainerLatestTag = pushContainerLatestTag;
            SignArtifacts = signArtifacts;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public void FailGit() => _git.SetCanceled();
        public void FailGitHub() => _gitHub.SetCanceled();
        public void FailChangelog() => _changelog.SetCanceled();
        public void FailDescription() => _description.SetCanceled();
        public void FailReleaseNotes() => _releaseNotes.SetCanceled();
        public void FailPullRequest() => _pullRequest.SetCanceled();

        public void FailOutstanding()
        {
            if (!_git.Task.IsCompleted)
            {
                _git.SetCanceled();
            }

            if (!_gitHub.Task.IsCompleted)
            {
                _gitHub.SetCanceled();
            }

            if (!_pullRequest.Task.IsCompleted)
            {
                _pullRequest.SetCanceled();
            }

            if (!_changelog.Task.IsCompleted)
            {
                _changelog.SetCanceled();
            }
        }
    }
}
