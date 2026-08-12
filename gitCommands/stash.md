# Git Stash

Stash lets you temporarily save uncommitted changes so you can switch context without committing half-done work.

## Common Scenario

You're working on a feature, but need to quickly check something on another branch:

```bash
# Save your current changes
git stash

# Switch branches, do your thing
git checkout main
# ... look at something ...

# Come back and restore your work
git checkout feature/your-branch
git stash pop
```

## Stash Commands

```bash
# Stash current changes (tracked files only)
git stash

# Stash with a description (helpful if you stash often)
git stash push -m "wip: booking validation logic"

# List all stashes
git stash list

# Apply most recent stash (keeps it in stash list)
git stash apply

# Apply and remove most recent stash
git stash pop

# Apply a specific stash
git stash apply stash@{2}

# Drop a specific stash
git stash drop stash@{0}

# Clear all stashes
git stash clear
```

## Including Untracked Files

By default, stash only saves tracked files. To include new untracked files:

```bash
git stash -u
```
