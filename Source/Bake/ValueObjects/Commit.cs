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

using YamlDotNet.Serialization;

namespace Bake.ValueObjects
{
    public class Commit
    {
        [YamlMember]
        public string Message { get; [Obsolete] set; } = null!;

        [YamlMember]
        public string Sha { get; [Obsolete] set; } = null!;

        [YamlMember]
        public Author Author { get; [Obsolete] set; } = null!;

        [YamlMember]
        public DateTimeOffset Time { get; [Obsolete] set; }

        [Obsolete]
        public Commit() { }

        public Commit(
            string message,
            string sha,
            DateTimeOffset time,
            Author author)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Message = message ?? string.Empty;
            Sha = sha ?? string.Empty;
            Author = author;
            Time = time;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public override string ToString()
        {
            return $"{Sha[..5]}: {Message}";
        }
    }
}
