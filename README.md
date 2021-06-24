# Bake

Bake is a convention based build tool that focuses on minimal to none effort
to configure and setup. Ideally you should be able to run `bake` in any
repository with minimal arguments and get the "expected" output or better. This
however comes at the cost of conventions and how well Bake works on a project
all depends on how many of the conventions that project follows.

Bake is the right tool for you if you

- ... don't want to have a complicated build setup and configuration
- ... just want to focus on the development part of your project
- ... want to have the "basics" covered during build and release
- ... just want a build, test and release process that works

Bake is **NOT** the right tool for you if you

- ... like having control of every part of the build and release process
- ... have a project with a lot of custom build and/or release steps

## Features

- **Artifacts** are automatically created for relevant projects. Examples
  are NuGet packages, Windows/Linux single binaries for tools, containers
  for `Dockerfile`
- **Release notes** are parsed and added to all applicable artifacts
- **Tests** are automatically located and executed

---

```
MIT License

Copyright (c) 2021 Rasmus Mikkelsen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
