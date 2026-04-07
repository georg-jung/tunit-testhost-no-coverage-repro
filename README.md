# Empty Cobertura Coverage with Microsoft.Testing.Extensions.CodeCoverage 18.5.2

## Bug Description

When using the **single test orchestrator** pattern with TUnit (where test classes live in a
separate class library and a dedicated `TestHost` project references them), the
`Microsoft.Testing.Extensions.CodeCoverage` collector (version 18.5.2, transitive from TUnit
1.23.7) produces **empty Cobertura XML** output: the `<packages />` element contains no package
entries, meaning zero coverage is reported.

The binary `.coverage` file IS generated and contains data (metadata), but when inspected with
`dotnet-coverage merge -f xml`, all assemblies (including the application code) are marked as
`path_is_excluded` in the `<skipped_modules>` section.

## Project Structure

```
Contoso.sln
  src/Contoso.Core/         -- regular class library (code under test)
  src/Contoso.Tests/        -- class library with TUnit test classes (NOT IsTestProject)
  src/Contoso.TestHost/     -- test orchestrator (IsTestProject=true, OutputType=Exe, refs TUnit)
```

`Contoso.TestHost` has a `ModuleInitializer` that force-loads `Contoso.Tests` so TUnit can
discover the tests. This is the "single test orchestrator" pattern recommended when multiple
test library projects exist, to avoid running multiple test host processes.

## Steps to Reproduce

```bash
# Build
dotnet build -c Release Contoso.sln

# Run tests with Cobertura coverage
dotnet test --no-build -c Release -- --coverage --coverage-output-format cobertura

# Inspect the Cobertura XML -- <packages /> is empty
cat src/Contoso.TestHost/bin/Release/net10.0/TestResults/*.cobertura.xml
```

The resulting Cobertura XML:

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<coverage line-rate="1" branch-rate="1" complexity="1" version="1.9" timestamp="...">
  <packages />
</coverage>
```

## Diagnostic: Inspecting the binary .coverage file

```bash
# Produce binary format instead
dotnet test --no-build -c Release -- --coverage

# Convert to XML to see skip reasons
dotnet-coverage merge -f xml src/Contoso.TestHost/bin/Release/net10.0/TestResults/*.coverage
```

This shows ALL assemblies are excluded:
```xml
<skipped_module name="contoso.core.dll" path="contoso.core.dll" reason="path_is_excluded" />
<skipped_module name="contoso.tests.dll" path="contoso.tests.dll" reason="path_is_excluded" />
<skipped_module name="contoso.testhost.dll" path="contoso.testhost.dll" reason="path_is_excluded" />
```

## What Was Tried (None Helped)

### Explicit `--coverage-settings` with XML config

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <ExcludeAssembliesWithoutSources>None</ExcludeAssembliesWithoutSources>
  <IncludeTestAssembly>True</IncludeTestAssembly>
  <CodeCoverage>
    <EnableDynamicManagedInstrumentation>True</EnableDynamicManagedInstrumentation>
    <ModulePaths>
      <Include>
        <ModulePath>.*\.dll$</ModulePath>
        <ModulePath>.*\.exe$</ModulePath>
      </Include>
    </ModulePaths>
  </CodeCoverage>
</Configuration>
```

Running with:
```bash
dotnet test -- --coverage --coverage-settings path/to/settings.xml
```

The settings are **completely ignored** — assemblies are still `path_is_excluded`.

### Explicit `--config-file` with testconfig.json

```json
{
  "codeCoverage": {
    "Configuration": {
      "Format": "cobertura",
      "ExcludeAssembliesWithoutSources": "None",
      "IncludeTestAssembly": true,
      "CodeCoverage": {
        "EnableDynamicManagedInstrumentation": true,
        "ModulePaths": { "Include": [".*\\.dll$"], "Exclude": [] }
      }
    }
  }
}
```

Running with:
```bash
dotnet test -- --config-file path/to/testconfig.json --coverage --coverage-output-format cobertura
```

Also **completely ignored**.

### Verbose profiler logging

Enabling profiler logging confirms modules ARE loaded:
```
[IM:...]OnModuleLoad: Contoso.TestHost.dll
[IM:...]OnModuleLoad: Contoso.Tests.dll
[IM:...]OnModuleLoad: Contoso.Core.dll
```

The profiler sees the modules, but the post-processing phase marks them as `path_is_excluded`.

### Using `dotnet-coverage collect` as external wrapper

```bash
dotnet-coverage collect -f cobertura "dotnet test --no-build -c Release"
```

Fails with "Profiler was not initialized" because the MTP extension's built-in coverage hook
blocks the external profiler.

### Using `coverlet.MTP` package

Adding `coverlet.MTP` 8.0.1 and using `--coverlet --coverlet-output-format cobertura` produces
no output files at all (tests pass but no coverage data is generated).

## Root Cause

The `Microsoft.Testing.Extensions.CodeCoverage` MTP extension (18.5.2) has a default setting of
`ExcludeAssembliesWithoutSources=MissingAll`. Despite the `.msCoverageExtensionSourceRootsMapping`
file being generated correctly by MSBuild targets (mapping source roots for all referenced
projects), the coverage post-processor still excludes all assemblies with `path_is_excluded`.

The `--coverage-settings` and `--config-file` flags appear to be non-functional — they do not
override the built-in defaults.

## Related Issues

- [microsoft/codecoverage#211](https://github.com/microsoft/codecoverage/issues/211) — Empty
  Cobertura reports (regression from 18.4.1), closed as fixed in 18.5.2 but still reproduces
- [microsoft/codecoverage#198](https://github.com/microsoft/codecoverage/issues/198) — Empty
  coverage results when `ContinuousIntegrationBuild=true`

## Environment

- .NET SDK: 10.0.100 (with `rollForward: latestMinor`)
- TUnit: 1.23.7
- Microsoft.Testing.Extensions.CodeCoverage: 18.5.2 (transitive from TUnit)
- OS: tested on Windows 11 Pro (10.0.26200)
