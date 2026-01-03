---
name: git-workflow
description: Complete git workflow for MauiPdfGenerator project following Conventional Commits
license: MIT
compatibility: opencode
metadata:
  audience: all-agents
  workflow: github
---

# Git Workflow Skill - MauiPdfGenerator

## What I Do

I guide you through the complete git workflow for this project, ensuring proper branching, commits, and PR creation following the project's Conventional Commits standard.

## When to Use Me

Use this skill when:
- Starting work on a new issue
- Creating branches from development
- Making commits with proper format
- Creating pull requests
- Need to understand the versioning impact of commits

---

## Branch Creation Workflow

### Step 1: Always Start from Development

```bash
# Update local development branch
git checkout development
git pull origin development

# Create feature branch (use issue number)
git checkout -b feature/42-short-description

# Or for fixes
git checkout -b fix/43-bug-description
```

**Branch Naming Convention:**
- `feature/N-description` - For new features (issue #N)
- `fix/N-description` - For bug fixes
- `docs/N-description` - For documentation work
- `refactor/N-description` - For refactoring

### Step 2: Verify Branch

```bash
# Confirm you're on correct branch
git branch --show-current

# Should output: feature/42-short-description
```

---

## Commit Format (Conventional Commits)

### Basic Structure

```
<type>[scope][!]: <description>

[optional body explaining what and why]

[optional footer with issue references]
```

### Types and Their Impact

| Type | Version | Use For |
|------|---------|---------|
| `feat:` | MINOR | New functionality |
| `fix:` | PATCH | Bug corrections |
| `feat!:` or `fix!:` | MAJOR | Breaking changes |
| `docs:` | None | Documentation only |
| `test:` | None | Test additions/changes |
| `refactor:` | None | Code restructure (no behavior change) |
| `perf:` | None | Performance improvements |
| `build:` | None | Build system changes |
| `ci:` | None | CI/CD configuration |
| `chore:` | None | Maintenance (use sparingly) |

### Scopes (REQUIRED)

| Scope | Package | Triggers Release? |
|-------|---------|-------------------|
| `(core)` | MauiPdfGenerator | ✅ Yes |
| `(diagnostics)` | MauiPdfGenerator.Diagnostics | ✅ Yes |
| `(sourcegen)` | MauiPdfGenerator.SourceGenerators | ✅ Yes |
| `(internal-task)` | Sample/, Docs/, .github/ | ❌ No |

### Commit Examples

**New Feature:**
```bash
git commit -m "feat(core): add table rendering with colspan support"
```

**Bug Fix:**
```bash
git commit -m "fix(diagnostics): resolve null reference in analyzer"
```

**Breaking Change:**
```bash
git commit -m "feat(core)!: change PdfDocument API to async

BREAKING CHANGE: All methods now return Task<T> instead of T.
Migration: Add 'await' to all PdfDocument method calls."
```

**Documentation:**
```bash
git commit -m "docs(internal-task): add table examples to README"
```

**Multiple Scopes = Multiple Commits:**
```bash
# ❌ WRONG
git commit -m "feat(core,diagnostics): add feature"

# ✅ CORRECT
git commit -m "feat(core): add table entity models"
git commit -m "feat(diagnostics): add table analyzer"
```

---

## Working with Issues

### Link Commits to Issues (Optional)

In commit body or footer:
```bash
git commit -m "feat(core): add table support

Implements basic table rendering with rows and columns.

Refs #42"
```

### Close Issues via PR (Recommended)

**Don't close in commits** - Let the PR do it:

```bash
# In commits, just develop
git commit -m "feat(core): add table models"
git commit -m "feat(core): implement table renderer"

# In PR description:
Closes #42
```

---

## Pull Request Workflow

### Step 1: Push Your Branch

```bash
git push origin feature/42-table-support
```

### Step 2: Create PR

**Via GitHub CLI:**
```bash
gh pr create --base development --title "feat(core): Add table support" --body "Closes #42

## Changes
- Added Table entity models
- Implemented TableRenderer
- Added unit tests (85% coverage)

## Testing
- All tests passing
- Manual testing with Sample app"
```

**Via GitHub Web:**
1. Go to repository
2. Click "New Pull Request"
3. Set base: `development` ← compare: `feature/42-table-support`
4. Add description with `Closes #42`

### Step 3: PR Description Template

```markdown
Closes #42

## Summary
[Brief description of changes]

## Changes
- Change 1
- Change 2
- Change 3

## Testing
- [ ] Unit tests added/updated
- [ ] Manual testing performed
- [ ] Documentation updated

## Breaking Changes
[If any, describe and migration steps]
```

---

## Versioning Logic

### Version Bump Priority

```
Breaking (!) > Feature (feat) > Fix (fix) > Others
     ↓              ↓              ↓          ↓
   MAJOR          MINOR          PATCH    No bump
```

### Examples

**Scenario 1:** PR with 5 `fix:` commits
```
Result: PATCH bump (1.5.11 → 1.5.12-preview)
```

**Scenario 2:** PR with 3 `fix:` + 2 `feat:` commits
```
Result: MINOR bump (1.5.11 → 1.6.0-preview)
Reason: feat > fix
```

**Scenario 3:** PR with fixes + feats + 1 `feat!:`
```
Result: MAJOR bump (1.5.11 → 2.0.0-preview)
Reason: breaking > feat > fix
```

**Scenario 4:** PR with only `docs:` and `test:` commits
```
Result: No release generated
Reason: No versioning types present
```

### Preview Versions

Commits to `development` generate preview versions:
```
1.6.0-preview-1   ← First relevant commit
1.6.0-preview-2   ← Second relevant commit
1.6.0-preview-3   ← Third relevant commit
```

Merge to `main` removes preview:
```
1.6.0-preview-3 → 1.6.0 (stable)
```

---

## Common Commands

```bash
# View commits since last stable tag
git log main-v1.5.11..HEAD --oneline -- MauiPdfGenerator/

# View only versioning commits (feat/fix)
git log --grep="^feat\|^fix" --oneline

# View commits by scope
git log --grep="feat(core)" --oneline

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Amend last commit message
git commit --amend

# View commit template
cat .gitmessage
```

---

## Troubleshooting

### "I committed to development directly"
```bash
# If not pushed yet
git reset --soft HEAD~1  # Undo commit, keep changes
git checkout -b feature/N-description
git commit -m "feat(scope): your message"
```

### "I need to change commit message"
```bash
# If not pushed
git commit --amend -m "feat(core): corrected message"

# If already pushed
# Better to leave it and be careful next time
```

### "Wrong scope in commit"
```bash
# If not pushed
git reset --soft HEAD~1
git commit -m "feat(correct-scope): message"
```

### "Forgot to link issue in PR"
Just edit the PR description and add `Closes #N`

---

## Workflow Summary

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Issue     │ ──▶ │   Branch    │ ──▶ │   Commits   │ ──▶ │     PR      │
│   Plan      │     │   (from dev)│     │   (proper)  │     │   Closes #N │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                     │
                                                                     ▼
                                                            ┌─────────────┐
                                                            │  CI Passes  │
                                                            └─────────────┘
                                                                     │
                                                                     ▼
                                                            ┌─────────────┐
                                                            │    Merge    │
                                                            │   to dev    │
                                                            └─────────────┘
                                                                     │
                                                                     ▼
                                                            ┌─────────────┐
                                                            │   Release   │
                                                            │  X.Y.Z-prev │
                                                            └─────────────┘
```

---

## Questions to Ask Yourself

Before committing:
- [ ] Is my commit type correct? (feat/fix/docs/etc.)
- [ ] Did I include the scope? (core/diagnostics/sourcegen/internal-task)
- [ ] Is this a breaking change? (add `!` if yes)
- [ ] Is the description clear and concise?
- [ ] Does this commit only affect ONE scope?

Before creating PR:
- [ ] Did I target `development` branch?
- [ ] Did I include `Closes #N` in description?
- [ ] Are all commits properly formatted?
- [ ] Did CI pass?

---

## Remember

- **One commit = One scope**
- **PR closes issues**, not commits
- **Breaking changes** need `!` and explanation
- **Preview versions** are automatic on `development`
- **Stable versions** only from `main`