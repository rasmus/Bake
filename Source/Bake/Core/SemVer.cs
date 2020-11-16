namespace Bake.Core
{
    public class SemVer
    {
        public static SemVer With(int major,
            int minor = 0,
            int patch = 0,
            string meta = null)
        {
            return new SemVer(major, minor, patch, meta);
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string Meta { get; }

        private SemVer(
            int major,
            int minor,
            int patch,
            string meta)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Meta = (meta ?? string.Empty).Trim('-');
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Meta)
                ? $"{Major}.{Minor}.{Patch}"
                : $"{Major}.{Minor}.{Patch}-{Meta}";
        }
    }
}
