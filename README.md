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

## Installing Bake

There is a few different ways to install Bake, choose one that best suites your
needs.

* **Download binary** - Simply download a binary from the
  [releases](https://github.com/rasmus/Bake/releases)
  page that suites your platform and architecture
* **Install .NET tool** - If have the .NET SDK installed, you can install
  Bake as a .NET tool.
  ```
  dotnet tool install --global Bake --version [VERSION]
  ```
  **NOTE:** Be sure to always install a specific version to ensure that your
  builds does not suddenly change behavior when new features are introduced
  in new versions of Bake.


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
bake run --convention=Release --destination="nuget>github,container>rasmus"
```

### Recognized repository content

When Bake analyzes a repository, it first gathers gathers basic information
regarding the environment its executed within.

* **Git** - Repository information like origin, branch name and commit hash are
  stored and used when creating e.g. release artifacts
* **GitHub** - If the repository origin originates from GitHub, the information
  is gathered and used to further enrich any release artifacts
* **GitHub Action** - When Bake is executed from within a GitHub action, it
  automatically recognizes the token and uses that when publishing artifacts
  and releases
* **Release notes** - If the repository contains a `RELEASE_NOTES.md` file,
  the content as well as the version information is used to further enrich
  any release artifacts

After the initial environment information gathering is completed, Bake starts
to scan the repository for files and structures it knows how to process.

* **[.NET](https://dot.net/)** - 
  Directories that contain [.NET](https://dot.net/) projects are
  analyzed and the application/service is built, tested and optionally
  put in a non-root and readonly filesystem compatible container
* **[Docker](https://www.docker.com/)** - 
  Directories that contain a
  [`Dockerfile`](https://docs.docker.com/engine/reference/builder/) will get
  the file built
* **[Go](https://go.dev/)** -
  Directories that contain Go projects are analyzed
  and the application/service is built, tested and optionally containerized
* **[Helm chart](https://helm.sh/)** -
  Helm charts are linted and packaged
* **[MkDocs](https://www.mkdocs.org/)** -
  MkDocs documentation sites are built and prepared as artifacts
* **[Python Flask](https://flask.palletsprojects.com/)** -
  Directories containing a Python Flask `app.py` file, will be bundled
  into a container

Based on the selected convention (by providing e.g. `--convention=Release`)
and the destinations for artifacts, Bake pushes/uploads/creates the built
artifacts to their configured destinations.

### Examples of sending artifacts to their destinations

Here are some examples of common used arguments to Bake

* `--destination=`
  * **Container**
    * `container>{username}`, e.g. simply `container>rasmus` - Will mark the
      destination as [Docker Hub](https://hub.docker.com/) with that username
    * `container>github` - Send containers to the GitHub package repository
      for at owner/organization of the git repository
    * `container>registry.local:5000` - Send containers to a specific container
      registry
  * **Helm**
    * `helm-chart>octopus@http://octopus.local/` - Sends Helm charts to the built-in
      repository in [Octopus Deploy][octopus-repository]. Bake looks for the API-key
      in an environment variable named `OCTOPUS_DEPLOY_APIKEY`
  * **NuGet**
    * `nuget` - An unnamed destination will send NuGet packages to the central
      NuGet repository at [nuget.org](https://www.nuget.org/). Bake will look for
      an API in an environment variable named `NUGET_APIKEY`
    * `nuget>github` - Send NuGet packages to the specific need with is owned
      by the owner of the repository of the current repository. Bake will
      automatically setup the API key for the current build using the
      `GITHUB_TOKEN` (automatically provided in GitHub actions), thus no
      additional configuration is required
    * `nuget>http://nuget.local/v3/index.json` - Send NuGet packages to the feed
      specified by the URL. Bake will look for the API key in an environment
      variable named `bake_credentials_nuget_{hostname}_apikey`, in which
      `{hostname}` is the hostname of the URL with invalid characters removed
  * **Release**
    * `release>github` - Creates release on GitHub within the current GitHub
      repository with the release notes and any important artifacts neatly
      bundled together in ZIP files with any `README.md`, `LICENSE` and 
      `RELEASE_NOTES.md` in ZIP files found in the root of the repository

## License

---

```
MIT License

Copyright (c) 2021-2022 Rasmus Mikkelsen

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

[octopus-repository]: https://octopus.com/docs/packaging-applications/package-repositories/built-in-repository
