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
using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class GitHubInformation
    {
        [YamlMember]
        public string Owner { get; [Obsolete] set; }

        [YamlMember]
        public string Repository { get; [Obsolete] set; }

        [YamlMember(typeof(string))]
        public Uri Url { get; [Obsolete] set; }

        [YamlMember]
        public Uri ApiUrl { get; [Obsolete] set; }

        [Obsolete]
        public GitHubInformation() { }

        public GitHubInformation(
            string owner,
            string repository,
            Uri url,
            Uri apiUrl)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Owner = owner;
            Repository = repository;
            Url = url;
            ApiUrl = apiUrl;
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}
