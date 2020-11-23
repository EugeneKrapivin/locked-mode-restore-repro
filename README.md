# Description

This project shows how to centrally manage package versions using Nuget + MSBuild + Directory.Build.target file.

## Repo description

the repo consists of 2 projects:

* `proj-a` - a net472 console project, with dependency on 3 packages (in csproj with no version) and a project reference on `proj-b`
* `proj-b` - a netstandard2.0 library, with dependency on 2 packages (in csproj with no version, mutually exclusive to `proj-a` dependencies) with no project references.

for both project Centralized Package Version Management (CPVM) is DISABLED and both project create package locks (`RestorePackagesWithLockFile` is set to true)

## Centralized Package Version Management and How it works

To create CPVM we have to know a little thing about how `.target` files are added to the build process.
Target files contain directives of tasks to enable in the build process, example of such a task is running `dotnet restore` during a build process.
We can use it, and the fact that target files are included into the build context only after the `csproj` and `build.props` are constructed to create CPVM.

### Directory.Build.target

This file contains the `<PackageReference>` elements with the package version. However, instead of `Include` we use an `Update` property when defining the package name. The reason for this will be explained shortly.

```xml
<?xml version="1.0" encoding="UTF-8" standalone="no" ?>
<Project>
  <ItemGroup>
    <PackageReference Update="Newtonsoft.Json" Version="[12.0.3, )" />
    <PackageReference Update="Newtonsoft.Json.Schema" Version="[3.0.13, )" />
    <PackageReference Update="TaskTupleAwaiter" Version="[1.2.0, 2)" />
    <PackageReference Update="UrlBase64" Version="[0.1.2, )" />
    <PackageReference Update="ZooKeeperNetEx" Version="[3.4.12.4, )" />
  </ItemGroup>
</Project>
```

### project files

Packages can be included directly in the `.csproj` file (as seen in this example) or in a `Build.props` file.
Notice that the `<PackageReference>` element includes the package name using the `Include` property (unlike the Update used in the `Build.target` file)
```xml
<?xml version="1.0" encoding="UTF-8" standalone="no" ?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net472</TargetFramework>
    <RootNamespace>proj_a</RootNamespace>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  </PropertyGroup>

  <ItemGroup Label="External">
    <PackageReference Include="Newtonsoft.Json" />
    <PackageReference Include="Newtonsoft.Json.Schema" />
    <PackageReference Include="TaskTupleAwaiter" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\proj-b\proj-b.csproj" />
  </ItemGroup>

</Project>
```
### How it all connects

During the build process the `.csproj` file is bundled with the local `Directory.Build.props` (if exists).
When the `.csproj` is built and bundled - the target files begin to execute. 
Once the `Directory.Build.targets` is executed, the values of the `<PackageReference Include="XXX">` matchup with the corresponding `<PackageReference Update="XXX">`
and the record in the bundled csproj is updated with the Version leading to a csproj containing the versions of the included package references.

> **Note**: Elements that do not match do not get added, this can be seen in the lock file.

## Creating a lock file

Lock files in nuget are per project (a feature to support a global lock file is on the way).
Per project lock files with CPVM allow us to get reproducible CI builds.

in order to instruct MSBuild to generate a lock file we have to include in the `csproj` (or in a central `Directory.Build.props` file) a property group that includes

```xml
<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
```

Now simlpy running
```cli
dotnet restore --no-cache
```
will create `package.lock.json` files in all projects where its enabled.

> **Note**: I've used `--no-cache` flag to ensure we are getting fresh packages, it is not stricly required on a local machine - it is recommended in CI builds  

> **Note**: You can consider to use `--force-evaluate` flag to ensure that lock files are regenerated.

### Sources on MSBuild and projects

https://www.youtube.com/watch?v=5HEbsyU5E1g&ab_channel=Pusher

## Using a lock file

Using a lock file to restore your dependencies could be done in 2 ways:

```cli
dotnet restore --no-cache --force --locked-mode
```

or by adding 

```xml
<PropertyGroup>
       <RestoreLockedMode Condition="'$(ContinuousIntegrationBuild)' == 'true'">true</RestoreLockedMode>
</PropertyGroup>
```

it is preferable that on the development machines we do not restore using the lock file to allow packages to update.
To enabled repeatable builds in a CI environment we can either call restore with the required flags or add a conditional element in the csproj.

> **Note**: more info on repeatable builds in https://devblogs.microsoft.com/nuget/enable-repeatable-package-restores-using-a-lock-file/ awesome blog post.

## Nuget version resolution - not what you think

There are lots of misconcesptions about how nuget version resolution works (like it takes always the lower bound of the range - wrong). I strongly suggest reading: https://docs.microsoft.com/en-us/nuget/concepts/dependency-resolution

## Running the project

First of all - what we've done is supported in VS, meaning once you load the solution VS will restore and build it automatically.

To run the project in CLI:

```cli
dotnet restore --no-cache --force --force-evaluate
dotnet build --no-restore
dotnet run --no-build --project .\proj-a\proj-a.csproj
```

to ensure that packages are restored from lock files (you first have to be sure you have lock files and that they are compatible with the dependencies as they are now):

```cli
dotnet restore --no-cache --force --locked-mode
dotnet build --no-restore
dotnet run --no-build --project .\proj-a\proj-a.csproj
```

If a lock file and the dependencies as defined in the csproj files are out of sync the restore will fail asking to update the lock files to ensure package compatibility with the package dependencies outlined in the csproj files.
