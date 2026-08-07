# Copilot Instructions

<!-- This file currently plays two roles at once: project rules (what AGENTS.md
would hold) and Copilot-specific behavior. See `.github\docs\adr\ADR-002-defer-agents-md.md` for why, and for
the trigger conditions that mean this should split back into a separate
AGENTS.md + a thin copilot-instructions.md. Check that file before this one
grows past ~150 lines or starts feeling like it's doing too much. -->

## Commands

<!-- Exact, copy-pasteable. Only include what Copilot couldn't guess from a default. -->

- Build: `...` 
- Test: `...`
- Lint: `...`

## Design Principles

<!-- Project-specific tradeoffs only — not restated general good practice. -->

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

## Copilot Review Behavior

<!-- This is the section that's genuinely specific to Copilot as a chat/review
tool, rather than a general project rule. Keep it distinct from Design
Principles above — this is about HOW Copilot should respond, not what the
project's technical decisions are. -->

- Respect the Design Principles and Boundaries above as settled decisions —
  don't re-litigate them in review comments.
- Do not suggest enterprise patterns (DI containers, generic repositories,
  event buses) unless explicitly asked.
- Prefer local, targeted fixes over architectural suggestions during review.
- ...

## Testing

<!-- Only if non-default: framework, mocking strategy, coverage threshold. -->

## Pointers

- Architecture / system design: `architecture.md`
- Current work, active decisions, in-flight status: `project-context.md`

## Documentation Map
 
<!-- This section guides Copilot on where documentation-content belongs. The
four-file test below is meant to be run by Copilot itself before writing new
documentation, to help it place content correctly— not just read passively. 
-->
 
Engineering docs are split by how often they change and what they contain. 
Don't restate content that already lives elsewhere — link to it instead. Use 
the four-file test below to determine where new content belongs.

**Four-file test — before writing anything new, ask:**
1. Is it a decision between named alternatives, with real tradeoffs? → ADR.
2. Is it a hard rule with no alternative ("never do X")? → Boundaries, above.
3. Is it acceptance criteria or expected behavior? → the current spec.
4. Is it how something is implemented or wired up? → `architecture.md`.
None of these fit → it's probably `project-context.md`, or doesn't need to be
written down at all.
