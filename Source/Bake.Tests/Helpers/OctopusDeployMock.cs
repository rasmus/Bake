// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
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
using System.Collections.Generic;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Bake.Tests.Helpers
{
    public class OctopusDeployMock : IDisposable
    {
        public static OctopusDeployMock Start()
        {
            return new OctopusDeployMock();
        }

        public Uri Url => new(Server.Url!);
        public string ApiKey { get; } = Guid.NewGuid().ToString("N");
        public WireMockServer Server { get; }
        public List<IRequestMessage> ReceivedPackages { get; } = new();

        private OctopusDeployMock()
        {
            Server = WireMockServer.Start();

            Server
                .Given(Request.Create()
                    .UsingPost()
                    .WithPath("/api/packages/raw")
                    .WithHeader("X-Octopus-ApiKey", ApiKey))
                .RespondWith(Response.Create()
                    .WithCallback(r =>
                    {
                        ReceivedPackages.Add(r);
                        return new ResponseMessage();
                    }));
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
