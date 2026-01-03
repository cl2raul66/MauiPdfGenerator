---
name: release-management
description: Versioning strategy and release process for MauiPdfGenerator suite
license: MIT
compatibility: opencode
metadata:
  audience: product-owner, devops-engineer
  workflow: github-releases
---

# Release Management Skill

## What I Do

I guide you through the versioning strategy and release process for the MauiPdfGenerator suite, including preview and stable releases with independent package versioning.

## When to Use Me

Use this skill when:
- Planning a release (preview or stable)
- Understanding version bump logic
- Creating release notes
- Managing multi-package versioning
- Transitioning from preview to stable

---

## Versioning Strategy

### Semantic Versioning (SemVer)

```
MAJOR.MINOR.PATCH[-preview-N]
  │     │     │        │
  │     │     │        └─ Preview counter (commits since stable)
  │     │     └────────── Bug fixes (backward compatible)
  │     └──────────────── New features (backward compatible)
  └────────────────────── Breaking changes (incompatible API)
```

### Version Bump Rules

**Priority (highest to lowest):**
```
Breaking Change (!) > Feature (feat) > Fix (fix) > Others
        ↓                  ↓              ↓          ↓
      MAJOR              MINOR          PATCH    No bump
```

---

## Independent Package Versioning

Each package versions independently based on its scope:

| Package | Scope | Current Version |
|---------|-------|-----------------|
| MauiPdfGenerator | `(core)` | Determined by `(core)` commits |
| MauiPdfGenerator.Diagnostics | `(diagnostics)` | Determined by `(diagnostics)` commits |
| MauiPdfGenerator.SourceGenerators | `(sourcegen)` | Determined by `(sourcegen)` commits |

### Example Scenario

**Commits in PR:**
- `feat(core): add table support`
- `fix(diagnostics): resolve analyzer bug`
- `docs(internal-task): update README`

**Result:**
- `MauiPdfGenerator`: 1.5.0 → 1.6.0-preview (MINOR bump from feat)
- `MauiPdfGenerator.Diagnostics`: 1.3.2 → 1.3.3-preview (PATCH bump from fix)
- `MauiPdfGenerator.SourceGenerators`: No change (no commits)

---

## Preview Releases (development branch)

### When They Happen

**Automatic** when PR merges to `development` with versioning commits (`feat`, `fix`, `feat!`, `fix!`).

### Preview Version Format

```
X.Y.Z-preview-N
```

Where `N` = number of relevant commits since last stable release.

### Example Evolution

```
Initial: 1.5.0 (stable on main)
  ↓
1.5.0-preview-1  ← First feat/fix commit to development
  ↓
1.5.0-preview-2  ← Second relevant commit
  ↓
1.5.0-preview-3  ← Third relevant commit
  ↓
1.5.0 (stable)   ← Merged to main, preview suffix removed
  ↓
1.6.0-preview-1  ← New cycle begins
```

### Preview Release Process

1. **PR merges to `development`**
2. **CI analyzes commits** for versioning types
3. **Determines bump** (MAJOR/MINOR/PATCH)
4. **Generates preview version** with counter
5. **Publishes to NuGet** with `-preview` tag
6. **Creates GitHub pre-release**

---

## Stable Releases (main branch)

### When They Happen

**Manual** when ready to release stable version from `development` to `main`.

### Stable Release Process

1. **Ensure `development` is ready:**
   ```bash
   # All tests passing
   dotnet test
   
   # All packages building
   dotnet build --configuration Release
   
   # Preview versions published
   # e.g., 1.6.0-preview-5
   ```

2. **Create release PR:**
   ```bash
   git checkout -b release/1.6.0
   git push origin release/1.6.0
   
   # Create PR: release/1.6.0 → main
   ```

3. **PR Title:** `release: v1.6.0`

4. **PR Description:**
   ```markdown
   Release version 1.6.0
   
   ## Changes Since Last Stable
   - feat(core): Added table rendering support
   - feat(core): Implemented column spanning
   - fix(diagnostics): Fixed null reference in analyzer
   - docs: Updated API documentation
   
   ## Breaking Changes
   None
   
   ## Package Versions
   - MauiPdfGenerator: 1.6.0
   - MauiPdfGenerator.Diagnostics: 1.3.3
   - MauiPdfGenerator.SourceGenerators: 1.2.0
   ```

5. **Merge to `main`:**
   - CI removes `-preview` suffix
   - Publishes stable packages to NuGet
   - Creates GitHub release with notes

6. **Sync back to development:**
   ```bash
   git checkout development
   git merge main
   git push origin development
   ```

---

## Version Bump Examples

### Scenario 1: Only Fixes

**Commits:**
```
fix(core): resolve rendering issue
fix(core): fix memory leak
fix(diagnostics): correct analyzer message
```

**Result:**
```
MauiPdfGenerator: 1.5.11 → 1.5.12-preview
MauiPdfGenerator.Diagnostics: 1.3.2 → 1.3.3-preview
```

---

### Scenario 2: Fixes + Features

**Commits:**
```
fix(core): resolve rendering issue
feat(core): add table support
fix(diagnostics): analyzer bug
```

**Result:**
```
MauiPdfGenerator: 1.5.11 → 1.6.0-preview (feat > fix)
MauiPdfGenerator.Diagnostics: 1.3.2 → 1.3.3-preview (only fix)
```

---

### Scenario 3: Breaking Changes

**Commits:**
```
feat(core)!: change API to async

BREAKING CHANGE: All synchronous methods removed.
Migration: Add 'await' to all method calls.
```

**Result:**
```
MauiPdfGenerator: 1.5.11 → 2.0.0-preview (breaking!)
```

---

### Scenario 4: No Versioning Commits

**Commits:**
```
docs(internal-task): update README
test(core): add unit tests
refactor(core): improve code structure
```

**Result:**
```
No packages released (no versioning commits)
```

---

## Release Notes Template

### For GitHub Releases

```markdown
# MauiPdfGenerator v1.6.0

## 🎉 New Features

### MauiPdfGenerator (v1.6.0)
- **Table Rendering**: Added support for HTML-like table rendering (#42)
  ```csharp
  pdfDocument.AddTable(table => 
      table.AddRow(row => row.AddCell("Header"))
  );
  ```
- **Column Spanning**: Implemented colspan support (#45)

### MauiPdfGenerator.Diagnostics (v1.3.3)
- **Improved Messages**: Analyzer messages now include fix suggestions (#48)

## 🐛 Bug Fixes

### MauiPdfGenerator (v1.6.0)
- Fixed memory leak in image rendering (#43)
- Resolved null reference in font loading (#44)

### MauiPdfGenerator.Diagnostics (v1.3.3)
- Fixed analyzer crash with nullable contexts (#47)

## 📚 Documentation
- Added table rendering examples to README
- Updated API documentation with async patterns

## 🔄 Migration Guide

### From 1.5.x to 1.6.0
No breaking changes. All existing code continues to work.

## 📦 Package Versions
- `MauiPdfGenerator`: 1.6.0
- `MauiPdfGenerator.Diagnostics`: 1.3.3
- `MauiPdfGenerator.SourceGenerators`: 1.2.0 (no changes)

## 📥 Installation

```bash
dotnet add package MauiPdfGenerator --version 1.6.0
```

## 🙏 Contributors
Thank you to all contributors!

---

**Full Changelog**: https://github.com/user/repo/compare/v1.5.11...v1.6.0
```

---

## Checking Version Impact

### View Commits Since Last Stable

```bash
# For core package
git log main-v1.5.11..development --oneline -- MauiPdfGenerator/

# Filter only versioning commits
git log main-v1.5.11..development --grep="^feat\|^fix" --oneline -- MauiPdfGenerator/
```

### Analyze Bump Type

```bash
# Check for breaking changes
git log main-v1.5.11..development --grep="!" --oneline

# Check for features
git log main-v1.5.11..development --grep="^feat" --oneline

# Check for fixes
git log main-v1.5.11..development --grep="^fix" --oneline
```

---

## Hotfix Process (Emergency Fixes)

### For Critical Bugs in Stable

1. **Branch from `main`:**
   ```bash
   git checkout main
   git checkout -b hotfix/1.5.12-critical-bug
   ```

2. **Make fix with proper commit:**
   ```bash
   git commit -m "fix(core): resolve critical security vulnerability"
   ```

3. **Create PR to `main`** (skip development):
   ```
   hotfix/1.5.12-critical-bug → main
   ```

4. **After merge, sync to development:**
   ```bash
   git checkout development
   git merge main
   git push origin development
   ```

---

## Pre-release Tags

### Alpha vs Beta vs Preview

We use **preview** exclusively:
- `1.6.0-preview-1` - Development builds
- `1.6.0-preview-2` - Subsequent development builds
- `1.6.0` - Stable release

**Note:** We don't use `alpha` or `beta` tags.

---

## Version Coordination Between Packages

### When to Keep Versions Aligned

**Not Required** - Each package versions independently.

**Example:**
```
MauiPdfGenerator: 2.3.5
MauiPdfGenerator.Diagnostics: 1.8.2
MauiPdfGenerator.SourceGenerators: 1.4.0
```

This is **normal and expected** since each evolves at its own pace.

### When to Coordinate

Only coordinate for **major breaking changes** that affect multiple packages:

```markdown
## Release v2.0.0 - MAJOR UPDATE

All packages updated to v2.0.0 for consistency.

### Breaking Changes Across Suite
- All APIs now async
- Configuration model changed
- Namespace reorganization
```

---

## CI/CD Integration

### Preview Release Workflow (development)

```yaml
name: Preview Release
on:
  push:
    branches: [development]

jobs:
  release:
    steps:
      - Analyze commits for versioning
      - Determine bump type (MAJOR/MINOR/PATCH)
      - Calculate preview counter
      - Update version in .csproj
      - Build and pack
      - Publish to NuGet with -preview tag
      - Create GitHub pre-release
```

### Stable Release Workflow (main)

```yaml
name: Stable Release
on:
  push:
    branches: [main]

jobs:
  release:
    steps:
      - Remove -preview suffix
      - Build and pack
      - Publish to NuGet (stable)
      - Create GitHub release
      - Generate release notes
```

---

## Rollback Strategy

### If Preview Release Has Issues

**Just fix and push** - preview counter increments:
```
1.6.0-preview-3 (broken)
  ↓ fix commit
1.6.0-preview-4 (fixed)
```

### If Stable Release Has Critical Issue

**Immediate hotfix:**
```
1.6.0 (broken) → hotfix branch → 1.6.1 (fixed)
```

**Do not revert stable release** - always forward with new version.

---

## Questions to Ask Before Release

### Before Preview Release (development)
- [ ] All tests passing?
- [ ] All relevant commits have proper conventional format?
- [ ] Each scope has appropriate commits?
- [ ] CI pipeline green?

### Before Stable Release (main)
- [ ] All features tested thoroughly?
- [ ] Documentation updated?
- [ ] Breaking changes documented with migration guide?
- [ ] Release notes prepared?
- [ ] All preview issues resolved?
- [ ] Team approval obtained?

---

## Common Scenarios

### "We want to skip preview and go straight to stable"

**Don't do this.** Preview releases allow:
- Community testing
- Early feedback
- Issue discovery before stable

### "Can we change a stable version?"

**No.** Once published to NuGet, versions are immutable. Use a new patch version instead.

### "Multiple packages need coordination"

Only for breaking changes. Otherwise, let them version independently based on their commits.

---

## Remember

- **Preview = development branch** (automatic)
- **Stable = main branch** (manual, deliberate)
- **Independent versioning** per package
- **Bump priority**: Breaking > Feature > Fix
- **Preview counter** increments with each relevant commit
- **Hotfixes** go to main, then sync to development