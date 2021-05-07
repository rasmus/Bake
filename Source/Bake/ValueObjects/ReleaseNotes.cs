using Bake.Core;

namespace Bake.ValueObjects
{
    public class ReleaseNotes
    {
        public SemVer Version { get; }
        public string Notes { get; }

        public ReleaseNotes(
            SemVer version,
            string notes)
        {
            Version = version;
            Notes = notes;
        }
    }
}
