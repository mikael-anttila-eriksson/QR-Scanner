---
description: Commit message guidelines for Conventional Commits.
loaded_by: .github/copilot-instructions.md
authoritative: true
---

## Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

## Rules

### Subject Line
- Use imperative mood ("Add" not "Added" or "Adding")
- Don't capitalize first letter after colon
- No period at the end
- Keep under 50 characters
- Be concise but descriptive

### Body (optional, use when needed)
- Separate from subject with a blank line
- Explain **what** and **why**, not **how**
- Wrap at 72 characters per line
- Use when:
  - Change is complex or non-obvious
  - Need to explain motivation or context
  - Multiple files affected for a single logical reason
  - Breaking changes need explanation

### Footer
- Add: Changes made by mae

## Types
- **feat:** New feature
- **fix:** Bug fix
- **docs:** Documentation only
- **style:** Code style (formatting, no logic change)
- **refactor:** Code restructuring (no behavior change)
- **perf:** Performance improvement
- **test:** Adding/updating tests
- **build:** Build system or dependencies
- **ci:** CI/CD configuration
- **chore:** Maintenance, non-production code
- **revert:** Revert previous commit

## Scope (optional)
- Module/feature: `auth`, `payment`, `checkout`
- Component: `navbar`, `button`, `modal`
- Technology: `webpack`, `npm`, `docker`
- Keep it short and lowercase

## Breaking Changes
- Add `!` after type/scope for breaking changes: `feat!:` or `feat(api)!:`

## Examples

### Simple (no body needed)
```
feat: Add email notifications
fix(auth): Correct token expiration handling
docs(readme): Update installation steps
```

### With body
```
refactor: Restructure component hierarchy

This simplifies the component tree and improves reusability
by extracting common UI elements into shared components.
```
```
fix: Prevent racing condition in checkout

Previously, rapid clicks on the submit button could create
duplicate orders. This adds a disabled state and debouncing
to prevent multiple submissions.
```
```
build: Rebuild compiled JavaScript from TypeScript

TypeScript source files were modified with new features.
This commit contains only the auto-generated JavaScript
output from compilation.
```

## Instructions
1. Analyze the code changes
2. Identify the primary type of change
3. Determine appropriate scope if applicable
4. Write a clear, concise subject line (under 50 characters)
5. Add a body if:
   - The change needs context or explanation
   - Multiple files changed for one logical reason
   - The "why" isn't obvious from the diff
6. If it's compiled/generated files, use `build:` or `chore:`

Generate the commit message in the proper format. Include a body only when it adds value.