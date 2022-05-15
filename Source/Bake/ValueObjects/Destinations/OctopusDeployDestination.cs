﻿// MIT License
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

namespace Bake.ValueObjects.Destinations
{
    [Destination(Names.Destinations.OctopusDeploy)]
    public class OctopusDeployDestination : Destination
    {
        [YamlMember(typeof(string))]
        public string Url { get; [Obsolete] set; }

        [Obsolete]
        public OctopusDeployDestination() { }

        public OctopusDeployDestination(
            string url)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Url = url;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public override string ToString()
        {
            return $"{Names.Destinations.OctopusDeploy}|{Url}";
        }
    }
}