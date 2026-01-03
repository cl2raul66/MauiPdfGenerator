---
name: dotnet-maui-setup
description: .NET MAUI specific commands and project setup for MauiPdfGenerator suite
license: MIT
compatibility: opencode
metadata:
  audience: developers
  technology: dotnet-maui
---

# .NET MAUI Setup & Commands Skill

## What I Do

I provide .NET MAUI specific commands, project structure guidance, and troubleshooting for the MauiPdfGenerator suite.

## When to Use Me

Use this skill when:
- Building, testing, or packing the libraries
- Working with multi-targeted projects
- Troubleshooting .NET MAUI specific issues
- Setting up development environment
- Understanding project structure

---

## Project Structure

```
MauiPdfGenerator/
├── MauiPdfGenerator/
│   ├── MauiPdfGenerator.csproj      # .NET MAUI class library
│   ├── Platforms/                   # Platform-specific code
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── Windows/
│   │   └── MacCatalyst/
│   └── *.cs                         # Shared code
│
├── MauiPdfGenerator.Diagnostics/
│   └── *.csproj                     # .NET Standard 2.0
│
├── MauiPdfGenerator.SourceGenerators/
│   └── *.csproj                     # .NET Standard 2.0 (Roslyn)
│
└── MauiPdfGenerator.Tests/
    └── *.csproj                     # xUnit tests (.NET 8+)
```

---

## Target Frameworks

### MauiPdfGenerator (Core)
```xml
<TargetFrameworks>
  net8.0-android;
  net8.0-ios;
  net8.0-maccatalyst;
  net8.0-windows10.0.19041.0
</TargetFrameworks>
```

### Diagnostics & SourceGenerators
```xml
<TargetFramework>netstandard2.0</TargetFramework>
```

### Tests
```xml
<TargetFramework>net8.0</TargetFramework>
```

---

## Essential Commands

### Build Commands

**Build all projects:**
```bash
dotnet build
```

**Build specific project:**
```bash
dotnet build MauiPdfGenerator/MauiPdfGenerator.csproj
dotnet build MauiPdfGenerator.Diagnostics/MauiPdfGenerator.Diagnostics.csproj
dotnet build MauiPdfGenerator.SourceGenerators/MauiPdfGenerator.SourceGenerators.csproj
```

**Build with specific configuration:**
```bash
dotnet build --configuration Release
dotnet build --configuration Debug
```

**Build for specific framework:**
```bash
dotnet build -f net8.0-android
dotnet build -f net8.0-ios
```

**Clean build:**
```bash
dotnet clean
dotnet build
```

### Restore Commands

**Restore dependencies:**
```bash
dotnet restore
```

**Force restore (clear cache):**
```bash
dotnet restore --force
dotnet restore --no-cache
```

### Test Commands

**Run all tests:**
```bash
dotnet test
```

**Run with coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Run specific test project:**
```bash
dotnet test MauiPdfGenerator.Tests/MauiPdfGenerator.Tests.csproj
```

**Run tests with detailed output:**
```bash
dotnet test --verbosity detailed
```

**Run specific test:**
```bash
dotnet test --filter "FullyQualifiedName~PdfDocumentTests.CreateDocument"
```

### Pack Commands (NuGet)

**Pack for NuGet:**
```bash
dotnet pack --configuration Release
```

**Pack specific project:**
```bash
dotnet pack MauiPdfGenerator/MauiPdfGenerator.csproj --configuration Release
```

**Pack with version:**
```bash
dotnet pack --configuration Release /p:Version=1.0.0
```

**Pack to specific output:**
```bash
dotnet pack --configuration Release --output ./artifacts
```

### Publish Commands

**Publish to NuGet (with API key):**
```bash
dotnet nuget push ./artifacts/*.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

**Publish to local feed:**
```bash
dotnet nuget push ./artifacts/*.nupkg \
  --source ~/.nuget/local-packages
```

---

## Working with Multi-Targeted Projects

### Build Specific Targets

```bash
# Build for Android only
dotnet build MauiPdfGenerator/MauiPdfGenerator.csproj -f net8.0-android

# Build for iOS only
dotnet build MauiPdfGenerator/MauiPdfGenerator.csproj -f net8.0-ios

# Build for Windows only
dotnet build MauiPdfGenerator/MauiPdfGenerator.csproj -f net8.0-windows10.0.19041.0

# Build for Mac Catalyst only
dotnet build MauiPdfGenerator/MauiPdfGenerator.csproj -f net8.0-maccatalyst
```

### Conditional Compilation

```csharp
#if ANDROID
    // Android-specific code
#elif IOS
    // iOS-specific code
#elif WINDOWS
    // Windows-specific code
#elif MACCATALYST
    // Mac Catalyst-specific code
#endif
```

---

## Project References

### Adding References Between Projects

```bash
# Add reference from Tests to Core
dotnet add MauiPdfGenerator.Tests/MauiPdfGenerator.Tests.csproj \
  reference MauiPdfGenerator/MauiPdfGenerator.csproj
```

### Adding NuGet Packages

```bash
# Add to specific project
dotnet add MauiPdfGenerator/MauiPdfGenerator.csproj package SkiaSharp

# Add specific version
dotnet add package SkiaSharp --version 2.88.3

# Add to solution (all projects)
dotnet add package Newtonsoft.Json
```

### Removing Packages

```bash
dotnet remove package PackageName
```

### Update Packages

```bash
# List outdated packages
dotnet list package --outdated

# Update specific package
dotnet add package PackageName
```

---

## Source Generators Specifics

### Building Source Generators

```bash
# Build generator project
dotnet build MauiPdfGenerator.SourceGenerators/MauiPdfGenerator.SourceGenerators.csproj

# Important: Clean consumer project when generator changes
dotnet clean MauiPdfGenerator/
dotnet build MauiPdfGenerator/
```

### Debugging Source Generators

Add to generator `.csproj`:
```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

View generated files:
```bash
ls obj/Generated/
```

---

## Diagnostics & Analyzers

### Building Diagnostics

```bash
dotnet build MauiPdfGenerator.Diagnostics/MauiPdfGenerator.Diagnostics.csproj
```

### Testing Analyzers Locally

Add to consumer project:
```xml
<ItemGroup>
  <ProjectReference Include="../MauiPdfGenerator.Diagnostics/MauiPdfGenerator.Diagnostics.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

---

## Common Issues & Solutions

### Issue: "The type or namespace name 'Maui' could not be found"

**Solution:**
```bash
# Ensure workload is installed
dotnet workload install maui

# Restore and rebuild
dotnet restore
dotnet build
```

### Issue: Build fails after generator changes

**Solution:**
```bash
# Clean is crucial for source generators
dotnet clean
dotnet build
```

### Issue: Multi-targeting errors

**Solution:**
```bash
# Build for each framework separately to identify issue
dotnet build -f net8.0-android
dotnet build -f net8.0-ios
dotnet build -f net8.0-windows10.0.19041.0
```

### Issue: Test discovery fails

**Solution:**
```bash
# Ensure test SDK is included in test project
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
```

### Issue: NuGet pack includes unwanted files

**Solution:**
Add to `.csproj`:
```xml
<PropertyGroup>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <IncludeBuildOutput>true</IncludeBuildOutput>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>

<ItemGroup>
  <None Include="README.md" Pack="true" PackagePath="/" />
</ItemGroup>
```

---

## Development Environment Setup

### Required Workloads

```bash
# Install .NET MAUI workload
dotnet workload install maui

# List installed workloads
dotnet workload list

# Update workloads
dotnet workload update
```

### Recommended VS Code Extensions

- C# Dev Kit
- .NET MAUI Extension
- NuGet Package Manager

### Recommended Visual Studio Workloads

- .NET Multi-platform App UI development
- .NET desktop development

---

## CI/CD Commands

### Used in GitHub Actions

```bash
# Restore
dotnet restore

# Build (all configurations)
dotnet build --configuration Release --no-restore

# Test with coverage
dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage"

# Pack
dotnet pack --configuration Release --no-build --output ./artifacts

# Publish to NuGet (in release workflow)
dotnet nuget push ./artifacts/*.nupkg \
  --api-key ${{ secrets.NUGET_API_KEY }} \
  --source https://api.nuget.org/v3/index.json
```

---

## Performance & Optimization

### Build Performance

```bash
# Parallel builds
dotnet build -m

# Skip restore if already done
dotnet build --no-restore

# Skip building dependencies
dotnet build --no-dependencies
```

### Clean Specific Outputs

```bash
# Clean all
dotnet clean

# Clean specific project
dotnet clean MauiPdfGenerator/MauiPdfGenerator.csproj

# Remove bin and obj manually
rm -rf **/bin **/obj
```

---

## Version Information

### Check installed versions

```bash
# .NET SDK version
dotnet --version

# List all SDKs
dotnet --list-sdks

# List all runtimes
dotnet --list-runtimes

# MAUI workload version
dotnet workload list
```

---

## Quick Reference

### Most Common Commands

```bash
# Daily workflow
dotnet restore          # Get dependencies
dotnet build           # Build everything
dotnet test            # Run tests
dotnet pack -c Release # Create NuGet packages

# Troubleshooting
dotnet clean           # Clean outputs
dotnet restore --force # Force restore
dotnet build --no-incremental # Full rebuild

# Specific targets
dotnet build -f net8.0-android      # Android only
dotnet test --filter "FullyQualified~TestName" # Specific test
```

---

## Remember

- **Always `dotnet clean`** after changing source generators
- **Multi-targeting builds** may need `-f` flag for specific platforms
- **Test coverage** requires `--collect:"XPlat Code Coverage"`
- **NuGet packs** should use `--configuration Release`
- **Source generators** output to `obj/Generated/` when configured