using System;
using System.Text.RegularExpressions;

namespace Bake.Core
{
    public class SemVer
    {
        private static readonly Regex VersionParser = new Regex(
            @"^(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d)){0,1}(\-(?<meta>[a-z0-9\-_]+)){0,1}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Random R = new Random();

        public static SemVer Random => new SemVer(
            R.Next(10, 100),
            R.Next(10, 100),
            R.Next(10, 100),
            "meta");

        public static SemVer Parse(string str)
        {
            var exception = InternalTryParse(str, out var version);
            if (exception != null)
            {
                throw exception;
            }

            return version;
        }

        public static bool TryParse(string str, out SemVer version)
        {
            return InternalTryParse(str, out version) == null;
        }

        public static Exception InternalTryParse(
            string str,
            out SemVer version)
        {
            version = null;
            if (string.IsNullOrEmpty(str))
            {
                return new ArgumentNullException(nameof(str));
            }

            var match = VersionParser.Match(str);
            if (!match.Success)
            {
                return new ArgumentException($"'{str}' is not a valid version string");
            }

            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = match.Groups["patch"].Success
                ? int.Parse(match.Groups["patch"].Value)
                : 0;
            var meta = match.Groups["meta"].Success
                ? match.Groups["meta"].Value
                : string.Empty;

            version = new SemVer(
                major,
                minor,
                patch,
                meta);

            return null;
        }

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
        public Version LegacyVersion { get; }

        private SemVer(
            int major,
            int minor,
            int patch ,
            string meta)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Meta = (meta ?? string.Empty).Trim('-');
            LegacyVersion = new Version(major, minor, patch);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Meta)
                ? $"{Major}.{Minor}.{Patch}"
                : $"{Major}.{Minor}.{Patch}-{Meta}";
        }
    }
}
