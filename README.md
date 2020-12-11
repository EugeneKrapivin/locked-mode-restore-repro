# Description

This repo shows how to use a [beta feature from nuget](https://github.com/NuGet/Home/wiki/Centrally-managing-NuGet-package-versions) to handle centralized package version management (CPVM)

## Repo description

the repo consists of 2 projects:

* `proj-a` - a net console project, with dependency on 3 packages (centrally managed) and a project reference on `proj-b`
* `proj-b` - a netstandard2.1 library, with dependency on 2 packages (centrally managed, mutually exclusive to `proj-a` dependencies) with no project references.

for both project CVPM is enabled and both project create package locks (`RestorePackagesWithLockFile` is set to true)

## Run it

```cli
dotnet run --project .\proj-a\proj-a.csproj
```

you should see an output:

```cli
[2020-12-11T22:21:56.1581216Z] - Did we make it?!
```

## How it works

Follow the [feature on nuget github](https://github.com/NuGet/Home/wiki/Centrally-managing-NuGet-package-versions)

Define the dependencies in the `csproj` just a you normally would, sans the `Version` element on the `<PackageReference>`.  
Create a `Directory.Package.props` file that will contain the `<PackageVersion>` elements for all packages used throughout the whole solution.  
Enable the feature in all the projects, prefereably in a centralized `Directory.Build.props` file (if you have multiple levels of props - [be sure to inherit](https://docs.microsoft.com/en-us/visualstudio/msbuild/customize-your-build?view=vs-2019#use-case-multi-level-merging))

### Notes

* Visual Studio UI still doesn't support it. Things like updates through UI will just write the `Version` property on the `<PackageReference>` elements in all the dependant `csproj`s
* For some bizzare reason, [dependency floating version](https://docs.microsoft.com/en-us/nuget/concepts/dependency-resolution#floating-versions) feature was disabled.
* Package locks feature **does not work** due to a bug, the reporuduction of which could be found in the [main branch of this repo](https://github.com/EugeneKrapivin/locked-mode-restore-repro/tree/main).
* Beta feature - will probably have changes in the future.
