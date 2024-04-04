# 0.25-beta

* *Nothing notable yet...*

# 0.24-beta

* Fix: Don't try to be clever and override default .NET GC settings

# 0.23-beta

* Feature: Output a container for hosting mkdocs documentation
* Feature: Add standard labels from the Open Containers Initiative
* Feature: Add dumb-init to .NET containers to ensure that the process
  and all child-processes are correctly terminated

# 0.22-beta

* New: GitHub releases now have a description that contains the a summary
  of all pull requests and issues that are part of the release
* Fixed: Cleanup of the artifact post run report

# 0.21-beta

* New: `GOPRIVATE` should be `direct` to ensure that removed
  dependencies are spottet and reduce information leackage
* New: Prints produced artifacts when done
* Fixed: GitHub URL information was wrong for GHE

# 0.20-beta

* Fixed: When publishing, `csproj` file is needed after 7.0.200

# 0.19-beta

* New: Clean `v|version` prefix for semantic versions
* New: Now possible to control the container name for .NET service using
  a `bake.yaml` file
* New: `DOTNET_ROLL_FORWARD` now defaults to `LatestMajor`
* Fixed: Container paths that contain `/` are now valid

# 0.18-beta

*  New: Expose go ldflags via environment variable
*  Fix: Fixed .NET warning `NETSDK1179`

# 0.17-beta

* New: Set some sensible values to `DOTNET_NOLOGO`,
  `DOTNET_SKIP_FIRST_TIME_EXPERIENCE`, `NUGET_XMLDOC_MODE`
  and `NUGET_EXE_NO_PROMPT`
* New: Basic support for NodeJS by looking for `package.json` files, building
  a container and using the script from the `main` property as entry

# 0.16-beta

* New: Ability to upload Helm charts to ChartMuseum

# 0.15-beta

* Fixed: Actually a working upload to Octopus Deploy

# 0.14-beta

* New: Allow naming of containers by creating a `bake.yaml` file besides
  the `Dockerfile`

# 0.13-beta

* New: Push Helm charts to Octopus Deploy raw package feed
* Fixed: .NET test reports now have unique names to ensure that reports
  are not overwritten if a projects targets multiple frameworks

# 0.12-beta

* Fixed: Not limits description of NuGet packages to 4.000 characters

# 0.11-beta

* New: Use content of `README.md` files as DLL and NuGet descriptions
* New: .NET test reports are now saved as `.trx` files allow artifact uploads
  to pick these up after build completion
* Fixed: Bake is now better at picking release notes with versions that
  are *similar* to the build version if no exact match can be found. As
  an example `1.0-alpha` is picked among `1.1`, `1.0-alpha` and `0.9`
  when the build version is `1.0.42`

# 0.10-beta

* Fixed: Project DLLs are now actually added to NuGet packages

# 0.9-beta

* New: If there is no `.dockerignore` file found when building a `Dockerfile`,
  create one with some sensible defaults
* New: By default, Docker builds will compress build context before sending
  it to the Docker daemon
* Fixed: Required `RepositoryUrl` parameter for NuGet packages is now
  the correct URL for the repository... for real this time

# 0.8-beta

* New: Now understands Python Flask applications and bundles these
  into containers
* Fixed: Required `RepositoryUrl` parameter for NuGet packages is now
  the correct URL for the repository

# 0.7-beta

* New: Produce ASP.NET Core containers that are able to run in read-only
  file systems, run as non-root as well as drop all capabilities. Here is
  an example security configuration
  ```yaml
  securityContext:
    runAsUser: 1000
    runAsGroup: 2000
    allowPrivilegeEscalation: false
    privileged: false
    readOnlyRootFilesystem: true
    capabilities:
      drop:
      - all
  ```
  * New: Now possible to change Bake internal defaults via environment
    variables. More of these will be exposed in upcomming releases
  * Fixed: Docker Hub push URL should just be the username

# 0.6-beta

* New: Containers and their tags are now listed on GitHub releases
* New: Helm charts are now linted and packaged

# 0.5-beta

* New: Now possible to set target platforms via --target-platform argument,
  which has the default win/x64, linux/x64 and osx/x64
* New: Bake now builds MkDocs built projects into ZIP files and releases
  them as part of the release process
* Fixed: Windows tool ZIP files now excludes the ".exe" in the filename
* Fixed: GitHub Enterprise usage should be detected in most cases now
* Fixed: Made sure that composers are do in an order that statisfies their
  needs

# 0.4-beta

* New: Run .NET in an Alpine Linux image instead of a full blow Ubuntu
* New: Provide descriptions of what each sub-command actually does
* Fixed: Not specifying a version should merely default to 1.0 instead
  of throwing an exception
* Fixed: Release files containing Bake should be named `bake` instead
  of `Bake`
* Fixed: Now correctly naming the ZIP filename of the release artifacts
  on GitHub releases

# 0.3-beta

* New: ZIP release artifacts before uploading them to GitHub releases
* Fixed: Release artifacts to GitHub releases are now correctly named
  after their release artifacts
* Fixed: .NET released DLLs/EXEs are now correctly stamped with version
* Fixed: Bake now correctly prints the help message if no sub-command
  is given instead of merely doing nothing

# 0.2-beta

* New: Build, test and create containers for either .NET or Go
* New: Supported destinations for artifacts
  * Docker Hub
  * GitHub Packages (NuGet and container)
  * GitHub Releases (tools)

# 0.1-alpha

- Basic functionality
