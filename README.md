# Description

There seems to be an issue using git CPVM and package locks.

## Repo description

the repo consists of 2 projects:

* `proj-a` - a net472 console project, with dependency on 3 packages (centrally managed) and a project reference on `proj-b`
* `proj-b` - a netstandard2.0 library, with dependency on 2 packages (centrally managed, mutually exclusive to `proj-a` dependencies) with no project references.

for both project CVPM is enabled and both project create package locks (`RestorePackagesWithLockFile` is set to true)

## Reproduce

```cli
dotnet restore --no-cache --force --force-evaluate
dotnet build --no-restore
dotnet run --no-build --project .\proj-a\proj-a.csproj
```

you should see an output:

```cli
[2020-11-16T21:30:09.5286941Z] - repro?!
```

meaning that while we do not try to restore using the package locks - the code is restoring and working.

However, attempting to restore in locked mode:

```cli
dotnet restore --no-cache --force --locked-mode
```

will result in an error:

```cli
Determining projects to restore...
C:\Program Files\dotnet\sdk\3.1.404\NuGet.targets(128,5): error NU1004: The packages lock file is inconsistent with the project dependencies so restore can't be 
run in locked mode. Disable the RestoreLockedMode MSBuild property or pass an explicit --force-evaluate option to run restore to update the lock file. [D:\code\lock-sln\lock-sln.sln]
  Failed to restore D:\code\lock-sln\proj-a\proj-a.csproj (in 99 ms).
  Restored D:\code\lock-sln\proj-b\proj-b.csproj (in 359 ms).
```

## Suspected error

If we carefully examin the [package lock file](https://github.com/EugeneKrapivin/locked-mode-restore-repro/blob/main/proj-a/packages.lock.json#L34) created for `proj-a` we will see an errornous listing of dependencies for the referenced `proj-b` project:

```json
"proj-b": {
  "type": "Project",
  "dependencies": {
    "Newtonsoft.Json": "12.0.3",
    "Newtonsoft.Json.Schema": "3.0.13",
    "TaskTupleAwaiter": "[1.2.0, 2.0.0)",
    "UrlBase64": "0.1.2",
    "ZooKeeperNetEx": "3.4.12.4"
  }
}
```

However, we only take dependency on `UrlBase64` and `ZooKeeperNetEx` in the `csproj` for `proj-b`.
If we manually delete:

```json
    "Newtonsoft.Json": "12.0.3",
    "Newtonsoft.Json.Schema": "3.0.13",
    "TaskTupleAwaiter": "[1.2.0, 2.0.0)",
```

The restore command in locked mode will succeed.

### References

https://github.com/NuGet/Home/issues/10074
