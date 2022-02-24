// MIT License
// 
// Copyright (c) 2021-2022 Rasmus Mikkelsen
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
using System.Text;
using System.Text.RegularExpressions;

namespace Bake.Core
{
    public class SemVer : IComparable<SemVer>, IEquatable<SemVer>, IComparable
    {
        private static readonly Regex VersionParser = new(
            @"^(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d+)){0,1}(\-(?<meta>[a-z0-9\-_]+)){0,1}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Random R = new();

        public static SemVer Random => new(
            R.Next(1000, 10000),
            R.Next(1000, 10000),
            R.Next(1000, 10000),
            R.Next(0, 2) == 0
                ? string.Empty
                : "meta");

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
                : null as int?;
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
        public int? Patch { get; }
        public string Meta { get; }
        public Version LegacyVersion { get; }
        public bool IsPrerelease => !string.IsNullOrEmpty(Meta);

        private readonly Lazy<string> _lazyString;

        private SemVer(
            int major,
            int minor,
            int? patch ,
            string meta)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Meta = (meta ?? string.Empty).Trim('-');
            LegacyVersion = patch.HasValue
                ? new Version(major, minor, patch.Value)
                : new Version(major, minor);

            _lazyString = new Lazy<string>(() =>
                new StringBuilder()
                    .Append($"{Major}.{Minor}")
                    .Append(Patch.HasValue ? $".{Patch}" : string.Empty)
                    .Append(!string.IsNullOrEmpty(Meta) ? $"-{Meta}" : string.Empty)
                    .ToString());
        }

        public SemVer WithMeta(string meta)
        {
            return new SemVer(
                Major,
                Minor,
                Patch,
                meta);
        }

        public override string ToString()
        {
            return _lazyString.Value;
        }

        public int CompareTo(object? obj)
        {
            return CompareTo(obj as SemVer);
        }

        public bool IsSubset(SemVer other)
        {
            if (other == null)
            {
                return false;
            }

            return Patch.HasValue
                ? Equals(other)
                : Major == other.Major &&
                  Minor == other.Minor;
        }

        public int CompareTo(SemVer other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;
            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;
            var patchComparison = Patch.GetValueOrDefault().CompareTo(other.Patch.GetValueOrDefault());
            if (patchComparison != 0) return patchComparison;
            return string.Compare(Meta, other.Meta, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(SemVer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                Major == other.Major &&
                Minor == other.Minor &&
                Patch == other.Patch &&
                string.Equals(Meta, other.Meta, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SemVer) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Major,
                Minor,
                Patch.GetValueOrDefault(),
                Meta);
        }

        public static bool operator ==(SemVer lhs, SemVer rhs)
        {
            if (lhs is { }) return lhs.Equals(rhs);
            return rhs is null;
        }

        public static bool operator !=(SemVer lhs, SemVer rhs) => !(lhs == rhs);
    }
}
