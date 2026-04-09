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
dotnet build Contoso.sln

# Run tests with Cobertura coverage
dotnet test --no-build --coverage --coverage-output-format cobertura

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
dotnet test --no-build --coverage

# Convert to XML to see skip reasons
dotnet-coverage merge -f xml src/Contoso.TestHost/bin/Debug/net10.0/TestResults/*.coverage
```

This shows ALL assemblies are excluded:
```xml
<skipped_module name="contoso.core.dll" path="contoso.core.dll" reason="path_is_excluded" />
<skipped_module name="contoso.tests.dll" path="contoso.tests.dll" reason="path_is_excluded" />
<skipped_module name="contoso.testhost.dll" path="contoso.testhost.dll" reason="path_is_excluded" />
```
