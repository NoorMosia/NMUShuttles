# Working with Remotes (Push, Pull, Fetch)

## Key Concepts

- **origin** — the default name for your remote repository (GitHub, Azure DevOps, etc.)
- **fetch** — download changes from remote without merging
- **pull** — fetch + merge in one step
- **push** — upload your local commits to the remote

## Daily Commands

```bash
# Pull latest changes from main
git pull origin main

# Push your current branch
git push

# Push a new branch for the first time (sets up tracking)
git push -u origin feature/your-branch-name
```

## Fetch vs Pull

```bash
# Fetch downloads remote changes but doesn't touch your working directory
git fetch origin

# You can then inspect what changed
git log origin/main --oneline -5

# Pull = fetch + merge into current branch
git pull origin main
```

Use `fetch` when you want to see what's new before integrating. Use `pull` when you're ready to integrate immediately.

## Handling Push Rejections

If your push is rejected because the remote has newer commits:

```bash
# Pull first (merge strategy)
git pull origin your-branch
# Resolve any conflicts, then push again
git push

# OR rebase strategy (cleaner history)
git pull --rebase origin your-branch
git push
```

## Checking Remote Info

```bash
# See configured remotes
git remote -v

# See what branches exist on the remote
git branch -r
```
