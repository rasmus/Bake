using System.Threading.Tasks;
using Bake.Core;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Ingredients
    {
        public static Ingredients New(
            string workingDirectory) => new Ingredients(
            SemVer.With(0, 1),
            new Credentials(),
            workingDirectory);

        [YamlMember(Alias = "version")]
        public SemVer Version { get; }

        [YamlMember(Alias = "credentials")]
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

        [YamlIgnore]
        public Task<GitInformation> GitTask => _git.Task;

        private readonly TaskCompletionSource<GitInformation> _git = new TaskCompletionSource<GitInformation>();

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
