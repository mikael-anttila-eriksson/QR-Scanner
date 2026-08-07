# AGENTS.md

> Project rules — how code is written in this repository. Changes rarely.
> Not for current work or system design — see Pointers at the bottom.

## Commands

<!-- Exact, copy-pasteable. Delete any line that's just a default (e.g. `npm test`) — only include what the agent couldn't guess. -->

- Build: `...`
- Test: `...`
- Lint: `...`

## Design Principles

<!-- Each line states a decision or tradeoff specific to THIS project, not general good practice. -->

- [ProjectName] is a [project type], not a [thing it's deliberately not].
- SaveAsync only persists. Concurrency is handled separately.
- Coalescing is intentionally use-first, not eager.
- Readability over DRY when abstraction would reduce clarity.

## Boundaries

### Always do
- ...

### Ask first
- Any architectural change (new layer, new pattern, cross-cutting refactor)
- Adding a new dependency
- ...

### Never do
- Recommend a generic abstraction solely for "consistency"
- Bypass the coalescing queue for writes
- ...

## Testing

<!-- Only if non-default: framework, mocking strategy, coverage threshold. Delete this section if your test setup needs no explanation. -->

## Pointers

- Architecture / system design: `architecture.md`
- Current work, active decisions, in-flight status: `project-context.md`
- Copilot-specific review/tone behavior: `.github/copilot-instructions.md`
