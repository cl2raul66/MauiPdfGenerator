---
name: mauipdf-generator
description: Agent for developing MauiPdfGenerator - a .NET MAUI PDF generation library with SkiaSharp. Expert in project structure, CI/CD workflows, Conventional Commits, and NuGet publishing.
license: MPL-2.0
compatibility: ">= 1.0.0"
---

# MauiPdfGenerator Agent Skill

## Project Overview

MauiPdfGenerator is a .NET MAUI library for generating PDF documents using SkiaSharp. The project follows a monorepo structure with three main components:

| Component | Path | Purpose | Package |
|-----------|------|---------|---------|
| Core | `MauiPdfGenerator/` | Main PDF generation library | ✅ NuGet |
| Source Generators | `MauiPdfGenerator.SourceGenerators/` | Roslyn source generators | ✅ NuGet |
| Tests | `MauiPdfGenerator.IntegrationTests/` | integration tests | ❌ |

## Branch Strategy

| Branch | Purpose | Merge Source |
|--------|---------|--------------|
| `development` | Main development branch | Feature branches |
| `master` | Stable releases | `development` only |

**Flow:**
```
Issue → Branch (from development) → Commits → PR → CI ✓ → Merge → Release
```

## Conventional Commits

All commits MUST follow the Conventional Commits format:

```
<type>[<scope>][!]: <description>

[cuerpo opcional]

[footer opcional]
```

### Types

| Type | Impact | When to Use |
|------|--------|-------------|
| `feat` | MINOR (+0.1.0) | New functionality |
| `fix` | PATCH (+0.0.1) | Bug fixes |
| `feat!`, `fix!` | MAJOR (+1.0.0) | Breaking changes |
| `docs` | No version bump | Documentation |
| `refactor` | No version bump | Code restructuring |
| `test` | No version bump | Tests |
| `perf` | No version bump | Performance improvements |
| `build` | No version bump | Build system |
| `ci` | No version bump | CI/CD configuration |
| `chore` | No version bump | Maintenance/mixed |

### Scopes

| Scope | Path | Description |
|-------|------|-------------|
| `core` | `MauiPdfGenerator/` | Core library (includes Diagnostics) |
| `sourcegen` | `MauiPdfGenerator.SourceGenerators/` | Source generators |
| `internal-task` | `Sample/`, `.github/` | Internal tasks |

### Examples

✅ **Correct:**
```
feat(core): add PDF table rendering support
fix(sourcegen): update FontAliasGenerator for new API
docs: update README with examples
ci: add CodeQL security workflow
```

❌ **Incorrect:**
```
feat: add tables everywhere  # Missing scope
feat(core): Added table support  # Past tense
```

### Commit Template

Configure once:
```bash
git config commit.template .gitmessage
```

## Issue Templates

Use the appropriate template for each issue:

| Template | Label | Use For |
|----------|-------|---------|
| `feature-request.yml` | `feat` | New functionality |
| `bug-report.yml` | `fix` | Bug reports |
| `maintenance.yml` | `chore` | Internal tasks (docs, refactor, test, ci, build, perf) |

Each template includes a "Plan de Implementación" checklist. Use commit prefixes in each task.

## Pull Request Template

File: `.github/PULL_REQUEST_TEMPLATE.md`

Required fields:
- Description
- Type of Change (check one or more)
- Issues Relacionados (Closes #XX)
- Plan de Implementación (checklist)
- Checklist de Merge

## CI/CD Workflows

### PR Validation (`pr-validation.yml`)
- Runs on PR to `development` or `master`
- Builds solution in Release
- Runs all tests (SourceGen, Core, Integration)
- Manual trigger via `workflow_dispatch`

### Development Release (`dev-release.yml`)
- Runs on push to `development`
- Publishes preview packages to NuGet
- Version format: `{major}.{minor}.{patch}-preview-{increment}`
- Creates preview tags: `main-v{major}.{minor}.{patch}-preview-{increment}`, `gen-v{major}.{minor}.{patch}-preview-{increment}`
- Preview tags are used by Production Release to promote to stable

### Production Release (`prod-release.yml`)
- Runs on push to `master` (or manual)
- Publishes stable packages to NuGet
- **New Feature:** Checks for pending preview versions (hybrid: local tags first, NuGet API fallback)
- Creates stable Git tags: `main-v{major}.{minor}.{patch}`, `gen-v{major}.{minor}.{patch}`
- Deletes preview tags after stable release
- Verifies NuGet publication (3 attempts, 10-minute intervals)
- Creates tracking issues with `status:preview-pending` label for manual verification

### CodeQL Security (`codeql.yml`)
- Runs on push/PR to `development` and `master`
- Analyzes C# code for security vulnerabilities
- Uploads results to GitHub Security tab

## Building and Testing

### Build Solution
```bash
dotnet build MauiPdfGenerator.sln --configuration Release
```

### Run Tests
```bash
dotnet test MauiPdfGenerator.sln --configuration Release
```

### Pack NuGet Packages
```bash
dotnet pack MauiPdfGenerator.SourceGenerators/MauiPdfGenerator.SourceGenerators.csproj --configuration Release
dotnet pack MauiPdfGenerator/MauiPdfGenerator.csproj --configuration Release
```

## Project Structure

```
MauiPdfGenerator/
├── .github/
│   ├── workflows/          # CI/CD workflows
│   ├── ISSUE_TEMPLATE/     # Issue templates
│   └── PULL_REQUEST_TEMPLATE.md
├── .opencode/
│   └── skill/mauipdf-generator/
├── MauiPdfGenerator/       # Core library
│   ├── Core/
│   ├── Fluent/
│   ├── Diagnostics/        # Merged into Core
│   └── MauiPdfGenerator.csproj
├── MauiPdfGenerator.SourceGenerators/
├── MauiPdfGenerator.IntegrationTests/
├── Sample/                 # Sample MAUI app
├── CONTRIBUTING.md         # Development guide
├── .gitmessage            # Commit template
├── MauiPdfGenerator.sln
└── global.json
```

## Common Tasks

### Create Feature Branch
```bash
git checkout development
git pull origin development
git checkout -b feature/42-description
```

### Create Fix Branch
```bash
git checkout development
git pull origin development
git checkout -b fix/42-brief-description
```

### Create Internal Task Branch
```bash
git checkout development
git pull origin development
git checkout -b chore/42-description
```

### Check Version Changes
```bash
# See commits since last stable
git log main-v1.5.11..HEAD --oneline -- MauiPdfGenerator

# See only feat/fix
git log main-v1.5.11..HEAD --grep="^feat\|^fix" --oneline
```

## Rules for AI Agent

1. **Always use Conventional Commits** - Check `.gitmessage` for format
2. **One scope per commit** - If affecting multiple scopes, split into multiple commits
3. **Link PR to Issue** - Use "Closes #XX" in PR description
4. **Run tests before commit** - Ensure all tests pass locally
5. **Update documentation** - If adding features, update relevant docs
6. **Follow the workflow** - Feature → PR → CI → Merge → Release
7. **Respect the branch strategy** - Never push directly to master
8. **Use project templates** - Issues, PRs, and commits must follow templates
9. **Run CodeQL** - Ensure security analysis passes before merge to master
10. **Update CONTRIBUTING.md** - If process changes, update the guide

## Version Bumping Logic

The version bump is determined by the **most significant** change in the PR:

```
Breaking (!) > Feature (feat) > Fix (fix) > Others
     ↓              ↓              ↓          ↓
   MAJOR          MINOR          PATCH    No bump
```

### Examples

| Commits in PR | Resulting Version |
|--------------|-------------------|
| 3 fixes | 1.5.11 → 1.5.12-preview |
| 5 fixes + 1 feat | 1.5.11 → 1.6.0-preview |
| fixes + feats + 1 breaking | 1.5.11 → 2.0.0-preview |
| only docs/chore/test | ❌ No release |

## Common CI/CD Errors and Solutions

### Development Release Issues

| Error                          | Solution                                        |
| ------------------------------ | ----------------------------------------------- |
| Preview tag not created        | Verify workflow has `contents: write` permissions |
| Package not published to NuGet | Check `NUGET_API_KEY` secret exists and is valid  |
| Wrong version increment        | Verify commits exist since last tag             |

### Production Release Issues

| Error                      | Solution                                                                              |
| -------------------------- | ------------------------------------------------------------------------------------- |
| Preview not promoted       | Check for preview tags: `git tag -l "*preview*"`                                        |
| Version calculation failed | Verify stable tags exist: `git tag -l "main-v*" "gen-v*"`                               |
| NuGet verification timeout | Manually verify at https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator |
| Tracking issue not created | Verify `status:preview-pending` label exists in repo settings                           |

### Agent Commands for Debugging

```bash
# Check for pending preview tags
git tag -l "*preview*"

# Check for stable tags
git tag -l "main-v*" "gen-v*"

# List issues with preview-pending status
gh issue list --label "status:preview-pending"

# Verify NuGet package exists
curl -s "https://api.nuget.org/v3-flatcontainer/{package-id}/index.json"
```

## References

- `.gitmessage` - Commit message template
- `CONTRIBUTING.md` - Full development guide
- `.github/workflows/` - CI/CD configuration
- `.github/ISSUE_TEMPLATE/` - Issue templates
- `.github/PULL_REQUEST_TEMPLATE.md` - PR template

## Important Notes

1. **Diagnostics is part of Core** - The separate `MauiPdfGenerator.Diagnostics` project was merged into `MauiPdfGenerator/Diagnostics/`. Use scope `core` for Diagnostics-related commits.
2. **Preview versions** - Development releases use `-preview-X` suffix
3. **Stable versions** - Only published from `master` branch
4. **Tags** - Generated automatically by workflows: `main-v*`, `gen-v*`

---

## GitHub CLI Integration

The agent uses GitHub CLI (gh) commands via bash tool to interact with Issues and Project Board.

**Requirements:**
- Install GitHub CLI: `winget install --id GitHub.cli`
- Authenticate: `gh auth login`

### Available GitHub CLI Commands

| Command | Usage |
|---------|-------|
| `gh issue list` | List issues (filterable by labels, assignee) |
| `gh issue create` | Create issue with template |
| `gh issue edit` | Update state, labels, assignee |
| `gh issue comment` | Add progress comments |
| `gh pr create` | Create PR with template |
| `gh pr merge` | Merge PR after review approval |

### Project Board

**URL:** https://github.com/users/cl2raul66/projects/6
**ID:** 6

### Complete Agent Workflow with GitHub CLI Integration

```
User: "Implement PDF table rendering support"
    ↓
Agent: bash gh issue list → Check if similar issue exists
    ↓
Agent: bash gh issue create → Create Issue #XX with feature-request.yml
    ├─ Auto-add labels: type: feature, scope: core
    └─ Title: "[Feat]: PDF table rendering support"
    ↓
Agent: bash gh issue edit → Assignee = cl2raul66
    ↓
Agent: bash gh issue comment → "Started work on this"
    ↓
git checkout development && git pull origin development
git checkout -b feature/xx-pdf-table-rendering
    ↓
Implement with Conventional Commits:
- feat(core): add PdfTable model
- feat(core): implement TableRenderer
- test(core): add TableRendererTests
    ↓
Agent: bash gh pr create → Create PR #YY
    ├─ Title: "feat(core): add PDF table rendering support"
    ├─ Body: Closes #XX
    └─ Labels: type: feature
    ↓
CI validates (pr-validation.yml + codeql.yml)
    ↓
Agent: bash gh issue comment → "Ready for review"
    ↓
Human review and approval
    ↓
bash gh pr merge → Merge to development
    ↓
Agent: bash gh issue comment → "Fixed in PR #YY"
Agent: bash gh issue close → state=closed
```

---

## Project Labels

Labels are critical for version bumping and issue management.

### Type Labels (Determine Version Bump)

| Label | Impact | Usage |
|-------|--------|-------|
| `type: feature` | MINOR | New functionality, retro-compatible |
| `type: breaking-change` | MAJOR | API changes that break compatibility |
| `type: maintenance` | No bump | Refactor, CI/CD, dependencies, tests |
| `type: docs` | No bump | Documentation only |

### Priority Labels

| Label | Usage |
|-------|-------|
| `priority: critical` | Blocker, requires immediate hotfix |
| `priority: high` | Required for next planned release |

### Scope Labels

| Label | Usage |
|-------|-------|
| `scope: core` | Affects MauiPdfGenerator (Core library) |

### Status Labels

| Label                  | Usage                                                            |
| ---------------------- | ---------------------------------------------------------------- |
| `status: on-hold`        | Temporarily paused                                               |
| `status:preview-pending` | Preview version published, awaiting manual verification in NuGet |

### Auto-Labeling Rules

When creating issues, apply these labels:

- **Feature Request** → `type: feature`
- **Bug Report** → `type: fix` (or create if missing)
- **Maintenance Task** → `type: maintenance`
- **Documentation** → `type: docs`
- **Core Component** → `scope: core`
- **High Priority** → `priority: high`
- **Critical Bug** → `priority: critical`

---

## New Agent Commands

### /new-issue
Create a new issue with appropriate template.

**Usage:**
```
/new-issue feat: soporte para tablas PDF
/new-issue fix: error al generar PDFs grandes
/new-issue chore: actualizar dependencias MAUI
/new-issue docs: actualizar README
```

**Agent Actions:**
1. bash gh issue list → Verify no duplicate
2. bash gh issue create → Select template (feature-request.yml, bug-report.yml, maintenance.yml)
3. bash gh issue edit → Assignee = cl2raul66, add labels

### /list-issues
List all open issues filtered by labels.

**Usage:**
```
/list-issues type: feature
/list-issues type: fix
/list-issues type: maintenance
```

**Output Format:**
```
| #   | Title                          | Labels                    |
|-----|--------------------------------|---------------------------|
| 42  | Add PDF table support          | type: feature, scope: core |
| 43  | Fix image rendering bug        | type: fix, priority: high  |
```

### /my-issues
List issues assigned to the current user (cl2raul66).

**Usage:**
```
/my-issues
```

**Agent Actions:**
1. bash gh issue list --assignee cl2raul66
2. Show status and progress for each issue
3. Link to Project Board status

### /plan-release
Plan tasks for a specific version release.

**Usage:**
```
/plan-release 1.7.0
```

**Agent Actions:**
1. bash gh issue list → Check existing issues for version
2. Identify missing tasks for the release
3. bash gh issue create → Create new issues using maintenance.yml
4. bash gh issue edit → Add priority: high to release blockers
5. Summarize release plan in comment

### /close-issue
Close an issue after PR merge.

**Usage:**
```
/close-issue 42 with PR #55
```

**Agent Actions:**
1. bash gh issue comment → "Fixed in PR #55"
2. bash gh issue close → state=closed

### /status-roadmap
Show current status of Project Board #6.

**Usage:**
```
/status-roadmap
```

**Output:**
```
MauiPdfGenerator Roadmap #6
━━━━━━━━━━━━━━━━━━━━━━━━━
TODO (5 issues):
  - #36: Implement word spacing
  - #42: PDF table support

IN PROGRESS (2 issues):
  - #44: Image optimization (cl2raul66)
  - #45: Font caching (cl2raul66)

DONE (28 issues):
- #43: Style resolution
- #41: Font alias generator
```

### /verify-preview-release
Verify and manage pending preview releases that need manual verification in NuGet.

**Usage:**
```
/verify-preview-release
/verify-preview-release status
/verify-preview-release cleanup
```

**Agent Actions:**
1. List preview tags: `git tag -l "*preview*"`
2. List tracking issues: `gh issue list --label "status:preview-pending"`
3. Check NuGet API for each pending preview
4. Show status of each preview (verified, pending, failed)
5. Provide commands to clean up resolved previews

**Output Format:**
```
Preview Release Status
━━━━━━━━━━━━━━━━━━━━━━
Pending Verifications (2):
  - gen-v1.3.4-preview-1: Issue #42 - Checking...
  - main-v1.6.0-preview-2: Issue #45 - Checking...

Verified Previews (1):
  - gen-v1.3.3-preview-5: Issue #40 - ✅ Verified, tag deleted

Cleanup Commands:
  # Delete resolved preview tags
  git tag -d gen-v1.3.3-preview-5
  git push origin :refs/tags/gen-v1.3.3-preview-5
```

**Sub-commands:**
- `status` - Show only verification status
- `cleanup` - Show commands to delete resolved preview tags
- `verify` - Force re-verification of all pending previews

---

## Auto-Assignment Rule

Always assign issues to the owner using:

```bash
gh issue edit XX --add-assignee cl2raul66
```

---

## Commit Messages That Close Issues

Use "Closes #XX" or "Fixes #XX" in commit footer:

```bash
feat(core): add PDF table rendering support

- Add PdfTable model
- Implement TableRenderer
- Add unit tests

Closes #42
```

This automatically links the commit to the issue and closes it when merged to master.

---

## Example Flows

### Flow 1: New Feature
```
User: /new-issue feat: add PDF table support

Agent:
1. bash gh issue list → No duplicate found
2. bash gh issue create → Created #42
3. bash gh issue edit → Assigned to cl2raul66, labels: type: feature, scope: core
4. git checkout -b feature/42-pdf-table-support
5. Makes commits
6. bash gh pr create → PR #55 linked to #42
7. After merge: closes #42
```

### Flow 2: Bug Fix
```
User: /new-issue fix: images not rendering on Android

Agent:
1. bash gh issue list → No duplicate found
2. bash gh issue create → Created #43 with bug-report.yml
3. bash gh issue edit → Assigned to cl2raul66, labels: type: fix, priority: high
4. git checkout -b fix/43-android-image-render
5. Makes commits
6. bash gh pr create → PR #56 linked to #43
```

### Flow 3: Maintenance Task
```
User: /new-issue chore: update .NET MAUI dependencies

Agent:
1. bash gh issue list → No duplicate found
2. bash gh issue create → Created #44 with maintenance.yml
3. bash gh issue edit → Assigned to cl2raul66, labels: type: maintenance
4. git checkout -b chore/44-update-maui-deps
5. Makes commits
```

### Flow 4: Release Planning
```
User: /plan-release 1.7.0

Agent:
1. bash gh issue list → Finds 5 open issues
2. Creates 3 new issues for missing features
3. Labels all with priority: high
4. Comments: "Release 1.7.0 plan:
   - [ ] #36: Word spacing
   - [ ] #42: PDF tables
   - [ ] #44: Image optimization
   - [ ] #45: Font caching
   - [ ] #46: New API (NEW)
   - [ ] #47: Performance (NEW)"
```

---

## GitHub API Reference

### Creating Issues

```bash
gh issue create --title "[Feat]: Feature description" --body "Use feature-request.yml template" --label "type: feature,scope: core"
```

### Updating Issues

```bash
gh issue edit 42 --add-assignee cl2raul66 --add-label "priority: high"
```

### Listing Issues

```bash
gh issue list --label "type: feature" --state open
```

### Adding Comments

```bash
gh issue comment 42 --body "Started work on this. Implementing..."
```

---

## Summary

This skill enables the agent to:
1. ✅ Manage GitHub Issues with proper templates and labels via GitHub CLI
2. ✅ Auto-assign issues to cl2raul66
3. ✅ Interact with Project Board #6
4. ✅ Create and merge Pull Requests
5. ✅ Follow complete development workflow
6. ✅ Ensure code quality with CI/CD and CodeQL

The agent is now a complete project manager and developer for MauiPdfGenerator, using GitHub CLI for all GitHub operations.
