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

using Bake.Core;
using Bake.Tests.Helpers;
using Bake.ValueObjects;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Bake.Tests.UnitTests.Core
{
    public class NuGetConfigurationTests : TestFor<NuGetConfiguration>
    {
        [Test]
        public async Task GenerateAsync()
        {
            // Arrange
            var nuGetSources = new[]
                {
                    new NuGetSource("github", "https://nuget.pkg.github.com/rasmus/index.json")
                };
            var credentialsMock = InjectMock<ICredentials>();
            credentialsMock
                .TryGetNuGetApiKeyAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())!
                .Returns(Task.FromResult("IllNeverTell"));

            // Act
            var xml = await Sut.GenerateAsync(nuGetSources, CancellationToken.None);

            // Assert
            xml.Should().Be(@"
<configuration>
  <packageSources>
    <clear />
    <add key=""github"" value=""https://nuget.pkg.github.com/rasmus/index.json"" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key=""Username"" value=""USERNAME"" />
      <add key=""ClearTextPassword"" value=""IllNeverTell"" />
    </github>
  </packageSourceCredentials>
</configuration>".Trim());
        }
    }
}
