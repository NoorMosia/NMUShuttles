# Git Basics

Essential commands you'll use every day.

## Setup (First Time)

```bash
# Configure your identity (do this once)
git config --global user.name "Your Name"
git config --global user.email "s229895735@mandela.ac.za"
```

## Checking Status

```bash
# See what's changed
git status

# See detailed changes (diff)
git diff

# See changes that are already staged
git diff --staged
```

## Staging and Committing

```bash
# Stage specific files (preferred)
git add src/Controllers/BookingController.cs
git add AppUI/lib/screens/home_screen.dart

# Stage all changes in a directory
git add Backend/

# Commit with a message
git commit -m "feat: add booking cancellation endpoint"
```

**Avoid `git add .` or `git add -A`** — it's easy to accidentally include files you didn't intend to commit (build artifacts, .env files, etc.).

## Viewing History

```bash
# Show recent commits
git log --oneline -10

# Show commits with file names changed
git log --stat -5

# Show a specific commit's details
git show <commit-hash>
```

## Undoing Things

```bash
# Unstage a file (keep changes in working directory)
git restore --staged filename.dart

# Discard changes to a file (revert to last commit)
git restore filename.dart

# Amend the last commit message (only if not pushed yet)
git commit --amend -m "corrected message"
```

## .gitignore

This project already has a `.gitignore`. If you create new files that shouldn't be tracked (secrets, build output, IDE settings), add patterns there before committing.

Common patterns already ignored:
- `bin/`, `obj/` (.NET build output)
- `.dart_tool/`, `build/` (Flutter build output)
- `.env` (environment secrets)
