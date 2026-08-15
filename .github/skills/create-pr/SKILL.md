---
name: create-pr
description: Copilot-driven PR creation skill. Makes a single, Copilot-followable set of steps to create a PR (feature -> dev or dev -> main) gather commits, synthesize a PR summary, insert it into the repo's PR template, and create the PR with gh.
local_only: true
reason: "This skill depends on repository-local files and temporary outputs; it must live inside the repository's .github/skills/create-pr/ folder so it can read/write temp files and access the repo's PR template."
required_files:
  - .github/skills/create-pr/SKILL.md
  - .github/skills/create-pr/temp_commits.txt
  - .github/skills/create-pr/temp_files.txt
  - .github/skills/create-pr/temp_pr_body.txt
required_templates:
  - .github/docs/templates/PULL_REQUEST_TEMPLATE.md
required_tools:
  - git
  - gh
environment_requirements:
  - Repository root must be a git repository with an "origin" remote.
  - Head branch must be pushed to origin before invoking this skill.
  - gh must be installed and authenticated (gh auth status --hostname github.com).
  - Process must have write permission to .github/skills/create-pr/ for temporary files.
notes: "Place and run this skill from the repository (not ~/.copilot/skills/) so it can access project-local templates and write temporary data."
---

# create-pr Skill

## Purpose

This skill gives Copilot a concrete, deterministic checklist it can follow from a single user prompt (e.g. "create PR from feature-mae to dev"). The workflow is designed to be fully automated except for Copilot/editor-driven PR summarization — it does NOT require external LLMs or manual multi-step intervention.

## Prerequisite

- The head branch must already be pushed to origin before invoking this skill. This skill will NOT push branches; it assumes the remote branch exists and will fail preflight if not.

## Supported flows

### This skill supports only:
- feature-mae -> dev
- dev -> main

Any other branch pair must fail immediately.

### This skill is create-only

Create-only behavior:
- This skill only creates new PRs.
- It never edits, updates, reopens, synchronizes, or force-pushes existing PRs.
- If a PR already exists for the same head/base pair, return the existing PR URL and stop.
- This skill never pushes local branches. The remote branch must already exist on origin.

## High-level process (step-by-step for Copilot)

### 1. Validate inputs (head, base)

Allowed pairs: feature-mae -> dev OR dev -> main. If unsupported, stop and report: "Unsupported branch pair: head='<head>' base='<base>'".

### 2. Preflight checks 

Preflight checks (stop on failure and report the failing command output):
   - git fetch origin <head> --quiet
   - git fetch origin <base> --quiet
   - git ls-remote --heads origin <head>  (must return a ref)
   - git ls-remote --heads origin <base>  (must return a ref)
   - gh --version  (ensure gh is installed)
   - gh auth status --hostname github.com  (ensure authenticated)
      If authentication fails:
      - stop immediately
      - report the command output
      - suggest running: gh auth login
   - Optional: gh repo view --json nameWithOwner (verify repository or accept --repo param)
   - git status --porcelain
      - If output is NOT empty:
      - stop immediately
      - report:
         "Uncommitted changes detected. Please commit or stash them before creating a PR."

### 3. Existing PR check (create-only behavior)

- Run:
  gh pr list --head <head> --base <base> --state open --json url,number --limit 1

- If an open PR already exists:
  - Return:
    "An open PR already exists: <url>"
  - Stop immediately.
  - Do NOT modify the PR.
  - Do NOT create another PR.

- If no PR exists:
  - Continue normally.

### 4. Collect repo data

Collect repo data (write to the skill folder so Copilot/editor can read it):
   - git rev-list --count origin/<base>..origin/<head>
      - If result is 0:
         - stop immediately
         - report:
            "No commits found between <base> and <head>. Nothing to create a PR for."
   - git log origin/<base>..origin/<head> --pretty=format:"%s%n%b%n---" > .github/skills/create-pr/temp_commits.txt
     (Each commit separated by a line with '---'; include full bodies so Copilot can infer intent.)
   - git diff --name-only origin/<base>..origin/<head> > .github/skills/create-pr/temp_files.txt

   Notes: 
   - Saving files under .github/skills/create-pr/ is required so the skill can be self-contained.
   - Temporary files may be overwritten on each run.

### 5. Synthesize PR title + description

Synthesize PR title + description using the local editor Copilot. Open both files in the editor and use the exact summarization prompt below. The goal is fully automatic: Copilot should output two items only, in this exact format (no extra commentary):

PR Title: <one-line short title suitable for gh --title>

PR Description:
<the description text that will be inserted into the PR template>

### 6. PR description requirements

PR description requirements (enforce in the prompt):
   - 3–6 bullets grouping related changes (CI/workflows, docs, skills, release automation, app code).
   - Mention added/modified top-level files/folders when relevant (e.g., .github/workflows/pr-main.yml).
   - Include a one-line "Scope" sentence and one-line "Reason" sentence.
   - Detect breaking changes by scanning commit bodies for "BREAKING CHANGE" or lines starting with "BREAKING:" and put them under a "### Breaking Changes" header or state "None".
   - Keep description 100–250 words.

### 7. Build PR body file

Build PR body file (automated):
   - Read repository file .github/docs/templates/PULL_REQUEST_TEMPLATE.md
   - Replace the token <!-- PR_DESCRIPTION --> with the PR Description (from step 5)
   - Remove the adjacent helper/placeholder description line if present (for example: "Provide a brief description of the changes in this PR.") so the inserted description is the only content in the Description section.
   - Replace the token <!-- PR_BreakingChanges --> with the breaking changes text found in step 6 (or "None").
   - Remove the adjacent helper/placeholder breaking-changes line if present (for example: "Describe any breaking changes or write \"None\".") so the Breaking Changes section contains only the replacement text.
   - Write result to .github/skills/create-pr/temp_pr_body.txt

### 8. Create the PR

Create the PR (fully automated, non-interactive — no preview):
   - gh pr create --base <base> --head <head> --title "<PR Title>" --body-file .github/skills/create-pr/temp_pr_body.txt --repo <owner/repo>
   - On failure, capture and report gh's stderr/stdout and stop.

### 9. Cleanup (optional)

Cleanup (optional): remove .github/skills/create-pr/temp_commits.txt, temp_files.txt, temp_pr_body.txt when finished.

## Copilot prompt (paste into editor with the two files open)

--- BEGIN PROMPT ---
Read the commit subjects and bodies (file: .github/skills/create-pr/temp_commits.txt) and the list of changed files (file: .github/skills/create-pr/temp_files.txt). Produce a PR Title and a concise PR Description suitable for insertion into .github/docs/template/PULL_REQUEST_TEMPLATE.md.

Output EXACTLY in this format (no surrounding text):

PR Title: <one-line short title>

PR Description:
<description text>

Requirements for PR Description:
- Use commit subjects/bodies to infer intent; do NOT paste raw commit lists.
- Group related changes into 3–6 bullets (short sentences).
- Mention modified top-level files/folders when relevant (e.g., .github/workflows/pr-main.yml).
- Add a one-line "Scope" sentence and a one-line "Reason" sentence.
- Detect breaking changes by searching commit bodies for "BREAKING CHANGE" or lines starting with "BREAKING:" and include them under a "### Breaking Changes" header at the end of the description; if none, write "None" under that header.
- Keep the description between 100 and 250 words.

Notes:
- The commit file uses '---' as a commit separator.
- The output will be programmatically inserted into the PR template. Do not include additional commentary or metadata.

--- END PROMPT ---

## Notes and guidance for Copilot

- This skill defines an "all-in-one" checklist that Copilot should follow when the human issues a single prompt like "create PR from dev to main". It is explicit about commands to run, files to create, and the exact prompt to use in the editor so the workflow can be automated without external LLMs or manual multi-step interactions.
- Keep temporary files inside .github/skills/create-pr/ so the skill is self-contained.
- If any preflight check fails, stop and return the failing command's output so the human can act.

## Examples (quick)

Run:
  git fetch origin feature-mae --quiet
  git log origin/dev..origin/feature-mae --pretty=format:"%s%n%b%n---" > .github/skills/create-pr/temp_commits.txt
  git diff --name-only origin/dev..origin/feature-mae > .github/skills/create-pr/temp_files.txt
Open both files and invoke Copilot with the prompt above to get "PR Title" and "PR Description", then run:
  gh pr create --base dev --head feature-mae --title "<PR Title>" --body-file .github/skills/create-pr/temp_pr_body.txt --repo <owner/repo>
