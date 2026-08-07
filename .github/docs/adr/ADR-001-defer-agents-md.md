---
title: ADR-001 - Defer AGENTS.md in Favor of copilot-instructions.md
status: Accepted
date: 2026-07-12
deciders:
  - Mae
related:
  - .github\copilot-instructions.md
  - .github\docs\project-context.md
  - .github\docs\reference\rules-reference.md
supersedes: null
superseded_by: null
tags:
  - architecture
  - ai-tooling
  - documentation
purpose: >
  Documents a significant architectural or engineering decision,
  including its context, rationale, consequences, and alternatives.
update_frequency: Never (immutable after Accepted, except status updates).
goal: Capture **why** a decision exists, not write a design document
---

# ADR-001: Defer AGENTS.md in Favor of copilot-instructions.md

## Status *(Required)*

**Accepted**

---

# Context *(Required)*

This repo is Copilot-only: development happens through Copilot Chat / agent
mode in VS Code, run locally. `.github/copilot-instructions.md` is the file
Copilot reliably loads for all chat requests in this mode.

AGENTS.md support is tied to GitHub's autonomous **Copilot coding agent** (the
cloud/unattended mode where an issue is assigned to Copilot and it opens a PR)
and to VS Code agent mode — not guaranteed across every Copilot surface. This
repo does not currently use the coding agent, and no second AI tool (Cursor,
Claude Code, Codex CLI, etc.) is in use against this codebase.

Maintaining both `AGENTS.md` and `copilot-instructions.md` now would mean two
files claiming the same "how do we write code here" territory, with no second
consumer to justify the duplication.

---

# Decision *(Required)*

Keep project rules, design principles, and review boundaries in
`.github/copilot-instructions.md`. Do not create `AGENTS.md` speculatively.

Create `AGENTS.md` the moment either becomes true:

1. This repo starts using GitHub's Copilot coding agent.
2. A second AI tool enters the picture (Cursor, Claude Code, Codex CLI, etc.),
   used by anyone against this repo.

If the trigger fires, `AGENTS.md` becomes the single source of truth for
project rules, and `copilot-instructions.md` either goes away or shrinks to a
pointer plus anything genuinely Copilot-specific (tone, review behavior). The
two are not maintained as parallel drafts of the same content — same
question, different consumer, one answer.

---

# Rationale *(Required)*

- **No second consumer yet.** AGENTS.md's value comes from being read by
  multiple tools/surfaces. With a single interactive Copilot surface, there is
  nothing else to share the file with.
- **Token cost of unused content.** Per the research behind
  `.github\docs\reference\rules-reference.md`, unnecessary always-loaded content measurably hurts
  agent performance, not just wastes disk space. An unused AGENTS.md sitting
  alongside a duplicate copilot-instructions.md is exactly that.
- **Avoiding drift.** Two files answering the same question, worded slightly
  differently, is how they quietly diverge and start giving different tools
  conflicting instructions for the same repo.

---

# Consequences *(Required)*

## Benefits

- One file to maintain instead of two; no risk of the two drifting apart.
- `copilot-instructions.md` stays the single, authoritative source for rules
  in the current (Copilot-only) setup.
- Clear, pre-agreed trigger conditions mean the decision doesn't need to be
  re-litigated later — just checked against.

## Drawbacks

- If a second AI tool or the coding agent is adopted, a migration step is
  required: extracting rules out of `copilot-instructions.md` into a new
  `AGENTS.md`.
- Until that trigger fires, any content written with only Copilot in mind may
  need light editing to be tool-agnostic once it moves.

## Risks

- Forgetting this decision exists and creating `AGENTS.md` speculatively
  anyway, recreating the duplication this ADR avoids.
- Forgetting to split when the trigger *does* fire, leaving Copilot-specific
  and general rules tangled together in one file indefinitely.

---

# Alternatives Considered *(Recommended)*

## Option A: Create AGENTS.md now, speculatively

### Pros
- Ready in advance if the coding agent or a second tool is adopted.
- One less migration step later.

### Cons
- No current consumer beyond what `copilot-instructions.md` already serves.
- Always-loaded content with no active benefit — measurable cost, no payoff.

### Why rejected
Speculative infrastructure with a known, checkable trigger condition is worse
than infrastructure created when the trigger fires. There's no forecasting
advantage here — the trigger is binary and easy to notice.

## Option B: Maintain both files in parallel now

### Pros
- Slight head start on separation.

### Cons
- Same content maintained twice, worded slightly differently each time.
- Actively invites drift with no current second consumer to justify it.

### Why rejected
This is precisely the failure mode described in the Rationale — two doors
into different rooms, not the same room.

---

# Implementation Notes *(Optional)*

- Rules currently live in `.github/copilot-instructions.md`, structured with
  Commands / Design Principles / Boundaries (Always / Ask first / Never) /
  Copilot Review Behavior / Pointers.
- Authoring guidance for what belongs under each heading lives in
  `.github\docs\reference\rules-reference.md`, written to apply to either file depending on which
  one is currently in use.

---

# Related Decisions *(Optional)*

None yet — this is the first ADR in this series.

---

# Future Considerations *(Optional)*

This ADR should be revisited (superseded, not edited) if:

- The Copilot coding agent is adopted for this repo.
- A second AI coding tool (Cursor, Claude Code, Codex CLI, etc.) is used
  against this repo by anyone.
- Copilot's own support model changes such that `copilot-instructions.md`
  stops being reliably loaded across surfaces in use here.

---

# References *(Optional)*

- GitHub Docs: [Creating Custom Agents](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
- Atlan, ["How to Write an AGENTS.md File"](https://atlan.com/know/how-to-write-agents-md/)
- Schmid, ["Writing a Good AGENTS.md"](https://www.philschmid.de/writing-good-agents)

---

# Revision History *(Optional)*

| Date | Change |
|------|--------|
| 2026-07-12 | Initial version, replacing loose `about_AGENTS.md` note |
| 2026-08-07 | Add to this project, from project at `repos/MAUI_ShoppingList/` |
