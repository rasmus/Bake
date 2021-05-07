using System;
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class GitInformation
    {
        [YamlMember(Alias = "sha")]
        public string Sha { get; }

        [YamlMember(typeof(string), Alias = "originUrl")]
        public Uri OriginUrl { get; }

        public GitInformation(
            string sha,
            Uri originUrl)
        {
            Sha = sha;
            OriginUrl = originUrl;
        }
    }
}
