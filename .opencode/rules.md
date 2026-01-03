# Project Context - MauiPdfGenerator Suite

This is a .NET MAUI class library suite for PDF generation with three main packages:
- **MauiPdfGenerator** - Core library (scope: `core`)
- **MauiPdfGenerator.Diagnostics** - Diagnostics tools (scope: `diagnostics`)
- **MauiPdfGenerator.SourceGenerators** - Source generators (scope: `sourcegen`)

All agents work as a coordinated team led by the Product Owner.

---

## Project Structure

```
MauiPdfGenerator/
├── MauiPdfGenerator/              # .NET MAUI class library
├── MauiPdfGenerator.Diagnostics/  # .NET Standard 2.0 class library
├── MauiPdfGenerator.SourceGenerators/ # Roslyn source generators
├── MauiPdfGenerator.Tests/        # xUnit test projects
├── Sample/                        # Sample applications
├── Docs/                          # Documentation
└── .github/                       # CI/CD workflows
```

---

## Target Frameworks

- **Core Library**: .NET 6+, .NET MAUI
- **Diagnostics**: .NET 6+
- **Source Generators**: .NET Standard 2.0
- **Tests**: .NET 8+ with xUnit

---

## Git Workflow (CRITICAL - Follow Strictly)

### Branch Strategy

```
main (stable releases)
  ↓
development (preview releases)
  ↓
feature/42-description  (work branches)
```

**Rules:**
1. **NEVER commit directly** to `main` or `development`
2. **Always branch from** `development`:
   ```bash
   git checkout development
   git pull origin development
   git checkout -b feature/42-short-description
   ```
3. **Create PR to** `development` (not `main`)
4. **Use `Closes #42`** in PR description to auto-close issues

---

## Commit Conventions (Conventional Commits)

### Structure
```
<type>[scope][!]: <description>

[optional body]

[optional footer]
```

### Types and Versioning Impact

| Type | Version Bump | When to Use |
|------|--------------|-------------|
| `feat:` | MINOR (+0.1.0) | New functionality |
| `fix:` | PATCH (+0.0.1) | Bug fix |
| `feat!:` or `fix!:` | MAJOR (+1.0.0) | Breaking change |
| `docs:` | None | Documentation only |
| `test:` | None | Tests only |
| `refactor:` | None | Code restructure (no behavior change) |
| `perf:` | None | Performance improvement |
| `build:` | None | Build system changes |
| `ci:` | None | CI/CD configuration |
| `chore:` | None | General maintenance (use sparingly) |

**Priority Rule:** Breaking > Feature > Fix > Others

### Scopes

| Scope | Package | Publishes NuGet? |
|-------|---------|------------------|
| `core` | MauiPdfGenerator | ✅ Yes |
| `diagnostics` | MauiPdfGenerator.Diagnostics | ✅ Yes |
| `sourcegen` | MauiPdfGenerator.SourceGenerators | ✅ Yes |
| `internal-task` | Sample/, .github/, Docs/ | ❌ No |

**Important:** One commit = One scope. If an issue affects multiple packages, make separate commits.

### Examples

✅ **Correct:**
```bash
git commit -m "feat(core): add table rendering support"
git commit -m "feat(sourcegen): update generator for tables"
git commit -m "docs(internal-task): add table example to README"
```

❌ **Incorrect:**
```bash
git commit -m "feat: add tables everywhere"  # Missing scope
git commit -m "feat(core,sourcegen): tables" # Multiple scopes
```

---

## Versioning System

### Independent Versioning
Each package versions independently based on its own scope commits.

### Preview Versions
Commits to `development` → `X.Y.Z-preview-N`
- `N` = number of relevant commits since last stable

### Stable Versions
Merge to `main` → `X.Y.Z` (removes `-preview`)

### Version Bump Examples

| Commits in PR | Version Change |
|---------------|----------------|
| 3 fixes | 1.5.11 → 1.5.12-preview |
| 5 fixes + 1 feat | 1.5.11 → 1.6.0-preview |
| fixes + feats + 1 breaking | 1.5.11 → 2.0.0-preview |
| only docs/test/chore | ❌ No release |

---

## Code Quality Standards

### Testing
- **Target Coverage**: 80%+ for all packages
- **Framework**: xUnit
- **Mocking**: Moq or NSubstitute
- **Required**: Unit tests for all public APIs

### Documentation
- **XML Comments**: Required on all public members
- **Examples**: Include usage examples in comments
- **README**: Update when adding features

### Code Style
- **Follow**: .NET coding conventions
- **Naming**: PascalCase for public, camelCase for private
- **Async**: Use `Async` suffix for async methods
- **Null Safety**: Enable nullable reference types

---

## .NET MAUI Specifics

### Build Commands
```bash
# Build all projects
dotnet build

# Build specific package
dotnet build MauiPdfGenerator/MauiPdfGenerator.csproj

# Run tests
dotnet test

# Pack for NuGet
dotnet pack --configuration Release
```

### Multi-targeting
When working with multi-targeted projects, ensure code works across all target frameworks.

---

## Available Resources

### MCP Servers
- **GitHub**: For issues, PRs, releases management
- **Microsoft Learn**: For .NET MAUI and .NET documentation

### Skills
- **git-workflow**: Complete git workflow for this project
- **dotnet-maui-setup**: .NET MAUI specific commands and setup
- **release-management**: Versioning and release process

---

## Issue Templates

Use GitHub issue templates:
- ✨ **New Feature** → `feat:` commits
- 🐞 **Bug Report** → `fix:` commits
- 🧰 **Internal Work** → `docs:`, `test:`, `ci:`, etc.

Each template includes a "Plan de Implementación" section to break down work into individual commits.

---

## Team Roles

See individual agent files in `.opencode/agent/` for detailed responsibilities:
- **Product Owner** (Primary) - Your main contact, coordinates all agents
- **Solution Architect** - Architecture and patterns
- **API Designer** - Public API design
- **Lead Library Developer** - Core implementation and mentoring
- **Library Developer** - Feature implementation
- **QA Engineer** - Testing and quality
- **Performance Engineer** - Optimization and profiling
- **Security Specialist** - Security audits
- **Technical Writer** - Documentation
- **DevOps Engineer** - CI/CD and releases

---

## Success Metrics

- **Code Coverage**: 80%+
- **NuGet Downloads**: Track adoption
- **GitHub Stars**: Community satisfaction
- **Breaking Changes**: Minimize frequency
- **Release Velocity**: Consistent cadence
- **Documentation**: Complete API reference

---

## Quick Reference

```bash
# Setup commit template
git config commit.template .gitmessage

# Create feature branch
git checkout -b feature/42-description

# View commits since last stable
git log main-v1.5.11..HEAD --oneline -- MauiPdfGenerator

# View only feat/fix commits
git log main-v1.5.11..HEAD --grep="^feat\|^fix" --oneline
```

---

**Remember:** When in doubt, consult the Product Owner. They coordinate the team and know the full context.