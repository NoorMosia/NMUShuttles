# Git Branching

Branching is the most important workflow concept for this project. Every feature, bugfix, or experiment should live on its own branch — never commit directly to `main`.

## Core Workflow

```bash
# 1. Make sure you're on main and up to date
git checkout main
git pull origin main

# 2. Create a new branch for your work
git checkout -b feature/your-feature-name

# 3. Do your work, stage, and commit
git add <specific-files>
git commit -m "feat: short description of what you did"

# 4. Push the branch to remote (first time)
git push -u origin feature/your-feature-name

# 5. Subsequent pushes on the same branch
git push

# 6. When done, create a Pull Request on GitHub/Azure DevOps
# After PR is merged, clean up locally:
git checkout main
git pull origin main
git branch -d feature/your-feature-name
```

## Branch Naming Conventions

| Prefix | Use Case | Example |
|--------|----------|---------|
| `feature/` | New functionality | `feature/student-booking-screen` |
| `fix/` | Bug fixes | `fix/otp-expiry-check` |
| `hotfix/` | Urgent production fixes | `hotfix/login-crash` |
| `chore/` | Maintenance, config, deps | `chore/update-flutter-deps` |
| `refactor/` | Code restructuring | `refactor/auth-service` |

## Keeping Your Branch Up to Date

When `main` has moved ahead while you're working on a feature branch:

```bash
# Option A: Merge main into your branch (simpler, creates merge commit)
git checkout feature/your-feature-name
git merge main

# Option B: Rebase onto main (cleaner history, but rewrite)
git checkout feature/your-feature-name
git rebase main
```

Use **merge** if the branch is shared with others. Use **rebase** for local-only branches you haven't pushed yet.

## Viewing Branches

```bash
# List local branches
git branch

# List all branches (including remote)
git branch -a

# See which branch you're currently on
git status
```

## Switching Between Branches

```bash
# Switch to an existing branch
git checkout branch-name

# If you have uncommitted changes that conflict, stash first
git stash
git checkout other-branch
# Come back later
git checkout feature/your-branch
git stash pop
```

## Deleting Branches

```bash
# Delete a local branch (safe — refuses if unmerged)
git branch -d branch-name

# Force delete a local branch (use with caution)
git branch -D branch-name

# Delete a remote branch
git push origin --delete branch-name
```

## Tips for This Project

- Always branch off `main` for new work.
- Keep branches short-lived — merge frequently to avoid big conflicts.
- One branch per concern: don't mix backend API changes with unrelated Flutter UI work on the same branch.
- Use descriptive commit messages: `feat:`, `fix:`, `chore:`, `docs:` prefixes help when scanning history.
