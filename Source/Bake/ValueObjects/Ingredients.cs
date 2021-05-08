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
        public SemVer Version { get; }

        [YamlIgnore]
        public Credentials Credentials { get; }

        [YamlMember(Alias = "workingDirectory")]
        public string WorkingDirectory { get; }

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

        public Ingredients(
            SemVer version,
            Credentials credentials,
            string workingDirectory)
        {
            Version = version;
            Credentials = credentials;
            WorkingDirectory = workingDirectory;
        }
    }
}
