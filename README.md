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


## Usage

Here are some examples of typical arguments passed to Bake.

### Basic test build

Here is the simple use case for using Bake on e.g. pull requests

```
bake run
```

### Basic release build

Here is the simple example of running a release build that sends NuGet packages
created during the release to the GitHub package store for the owner of the
current repository.

```
bake run --convention=Release --destination="nuget>github"
```

### Examples of common used arguments

Here are some examples of common used argumnets to Bake

* `--destination=`
  * **Container**
    * `container>{username}`, e.g. simply `container>rasmus` - Will mark the
      destination as [Docker Hub](https://hub.docker.com/) with that username
    * `container>github` - Send condcontainers to the GitHub package repository
      for at owner/organization of the git repository
    * `container>localhost:5000` - Send containers to a specific container
      registry
  * **NuGet**
    * `nuget` - An unamed destination will send NuGet packages to the central
      NuGet repository at [nuget.org](https://www.nuget.org/). Bake will look for
      an API in an environment variabled named `NUGET_APIKEY`
    * `nuget>github` - Send NuGet packages to the specific need with is owned
      by the owner of the repositry of the current repository. Bake will
      automatically setup the API key for the current build using the
      `GITHUB_TOKEN` (automatically provided in GitHub actions), thus no
      additional configuration is required
    * `nuget>http://localhost:5555/v3/index.json` - Send NuGet packages to the feed
      specified by the URL. Bake will look for the API key in an environment
      variable named `bake_credentials_nuget_{hostname}_apikey`, in which
      `{hostname}` is the hostname of the URL with invalid characters removed
  * **Release**
    * `release->github` - Creates release on GitHub within the current GitHub
      repository with the release notes and any important artifacts neatly
      bundled together in ZIP files with any `README.md`, `LICENSE` and 
      `RELEASE_NOTES.md` in ZIP files found in the root of the repository

## License

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
