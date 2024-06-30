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

using Bake.ValueObjects.Artifacts;
using Bake.ValueObjects.Recipes.MkDocs;

namespace Bake.Cooking.Cooks.MkDocs
{
    public class MkDocsDockerFileCook : Cook<MkDocsDockerFileRecipe>
    {
        private const string Dockerfile = @"
FROM alpine:3.13.2 AS builder

ARG THTTPD_VERSION=2.29

RUN \
  apk add gcc musl-dev make && \
  wget http://www.acme.com/software/thttpd/thttpd-${THTTPD_VERSION}.tar.gz && \
  tar xzf thttpd-${THTTPD_VERSION}.tar.gz && \
  mv /thttpd-${THTTPD_VERSION} /thttpd && \
  cd /thttpd && \
  ./configure && \
  make CCOPT='-O2 -s -static' thttpd && \
  adduser -D static

FROM scratch

EXPOSE 8080

COPY --from=builder /etc/passwd /etc/passwd
COPY --from=builder /thttpd/thttpd /

USER static
WORKDIR /home/static

COPY {{PATH}} .

CMD [""/thttpd"", ""-D"", ""-h"", ""0.0.0.0"", ""-p"", ""8080"", ""-d"", ""/home/static"", ""-u"", ""static"", ""-l"", ""-"", ""-M"", ""60""]
";

        protected override async Task<bool> CookAsync(
            IContext context,
            MkDocsDockerFileRecipe recipe,
            CancellationToken cancellationToken)
        {
            var dockerFilePath = recipe.Artifacts
                .OfType<DockerFileArtifact>()
                .Single()
                .Path;;

            var dockerfileContent = Dockerfile
                .Replace("{{PATH}}", recipe.Directory);

            await File.WriteAllTextAsync(
                dockerFilePath,
                dockerfileContent,
                cancellationToken);

            return true;
        }
    }
}
