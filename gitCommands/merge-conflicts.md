# Handling Merge Conflicts

Conflicts happen when two branches modify the same lines. Git can't decide which version to keep, so it asks you.

## When Do Conflicts Occur?

- Merging a branch: `git merge feature/x`
- Pulling from remote: `git pull origin main`
- Rebasing: `git rebase main`

## What a Conflict Looks Like

Git marks conflicted sections in the file:

```
<<<<<<< HEAD
// Your current branch's version
var greeting = "Hello";
=======
// The incoming branch's version
var greeting = "Hi there";
>>>>>>> feature/other-branch
```

## How to Resolve

1. Open the conflicted file(s) — VS Code/Kiro highlights them.
2. Choose which version to keep (or combine both).
3. Remove the conflict markers (`<<<<<<<`, `=======`, `>>>>>>>`).
4. Stage the resolved file:
   ```bash
   git add resolved-file.dart
   ```
5. Complete the merge:
   ```bash
   git commit
   ```
   (Git auto-fills a merge commit message.)

## Aborting a Merge

If things get too messy and you want to start over:

```bash
git merge --abort
```

## Preventing Conflicts

- Pull from main often — small, frequent merges are easier than big ones.
- Communicate with teammates about which files you're working on.
- Keep branches focused on one concern so changes don't overlap.
