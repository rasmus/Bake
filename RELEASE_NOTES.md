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

# 0.2-beta initial release

* New: Build, test and create containers for either .NET or Go
* New: Supported destinations for artifacts
  * Docker Hub
  * GitHub Packages (NuGet and container)
  * GitHub Releases (tools)

# 0.1-alpha version of Bake

- Basic functionality
