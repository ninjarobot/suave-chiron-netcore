Suave-Chiron-Netcore
====================

Project demonstrating basic use of Suave and Chiron running on the .NET Core
runtime, including tests.

Building, Testing, and Running
--------
On a *nix platform with `make` installed, you can use the Makefile to 
build and clean, such as `make`, `make check`, and `make clean`.

### Building
Change to the `SuaveChironNetcore` directory and execute 
`dotnet restore && dotnet build`.  This will restore any dependencies and build
the main assembly.

### Testing
Change to the `SuaveChironNetcoreTests` directory and execute `dotnet restore`
to restore any dependencies and then `dotnet test` to execute the tests defined
in the `HNProviderTests.fs` module.

### Running
Change to the `SuaveChironNetcore` directory and execute `dotnet run`, after 
which the HTTP services are listening on port 8083, such as 
http://localhost:8083/topstories.


Gotchas
-------
* Using `dotnet new -t xunittest` currently only supports C#, but it's a useful
starting point anyway.  Edit the project.json for F#:
    + under `buildOptions`:
    ```
        "compilerName": "fsc",
        "compile": {
            "includeFiles": [
                "Tests.fs"
            ]
        }
    ```
    + Add `tools`: 
    ```
      "tools": {
          "dotnet-compile-fsc": "1.0.0-preview2.1-*"
        },
    ```
    + Under `framework/dependencies/netcoreapp1.1`:
    ```
        "Microsoft.FSharp.Core.netcore": "1.0.0-alpha-*"
    ```
    + And finally rename Tests.cs to Tests.fs and change it to F# syntax.

* In the test project's `project.json`, import the other project's directory
to reference it for testing by adding it under `dependencies`:
    ```
        "SuaveChironNetcore":"1.0.0-*"
    ```
* Whenever adding files, you'll need to add them to the `compile` section
of `project.json` in the order they should be built.
* The build for xunit may give a warning about the version.  You need some RC2 
version, just set the dependency to `"dotnet-test-xunit": "1.0.0-rc2-*",`.