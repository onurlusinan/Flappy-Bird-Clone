---
name: feature-workflow
description: The end-to-end loop for delivering a change in this repo - branch off dev, agree the design, implement, verify in Unity, hand the user a concrete play-test checklist, commit, and open the PR against dev (master only for releases). Use this at the start of any new feature, fix or refactor in this project, and when the user asks to commit, push, open a PR, release, or "ship" something.
---

# Feature workflow

## Branching (the user's rules)

- `dev` is the main development branch.
- Each piece of work gets its own branch cut from up-to-date `dev`: `feature/<short-name>` (or `fix/<short-name>`).
- Work merges back into `dev` through a PR.
- `master` only receives major updates, merged from `dev`, when the user says it's a release. Never branch from or merge features straight into `master`.
- `deprecated/og` (original 2020 project) and `deprecated/dev-2021` (abandoned remake) are archives: don't modify them.

Before starting: `git status` must be clean (or the user's pending changes understood), then `git switch dev && git pull` (if possible) and `git switch -c feature/<name>`.

## The loop

1. **Agree the design** when the change has real choices (gameplay rules, UI flow, online model, new dependencies). Present a short plan with a recommendation; implement once agreed. Pure bug fixes and mechanical cleanups can go straight ahead.
2. **Implement** following `unity-dev` (conventions, batchmode editing) and `game-code-design` (ownership, events, states). Online work follows `online-multiplayer`.
3. **Verify** with the strongest checks available (compile → static scan → scripted play-through) and report exactly which ran and what they cover.
4. **Hand over a play-test checklist**: the specific things the user should see/feel when pressing Play (visuals, feel, audio, mobile layout). Automated checks render nothing, so this step is not optional for anything visible.
5. **Commit** in logical chunks: stage explicit paths (never `git add -A` on a dirty tree without reviewing it), include `.meta` files with their assets, leave out Unity churn the change didn't cause. Message: imperative summary under ~72 chars, blank line, body explaining what changed and why grouped by area, then the attribution line from the system prompt.
6. **PR against `dev`**: title + description with sections for what changed, why, testing (including what was *not* verified), and follow-ups. Keep it about the project - don't mention other repositories or tooling the user wants left out.

## Pushing

This shell may not have GitHub credentials (the user often pushes from the Fork app). If a push fails on auth, give the user the exact `! git push ...` command instead of retrying. Never force-push or delete remote branches without the user's explicit go-ahead, and back up anything that would be overwritten first.
