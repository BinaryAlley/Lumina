---
name: csharp-conventions
description: Project-specific C# coding conventions for naming, formatting, documentation, and domain patterns. Load this before writing or reviewing any .cs file. Distinguishes rules already enforced by .editorconfig as build errors from judgment-based rules that require manual review.
---

# C# Conventions

> This file is the canonical, tracked copy of the `csharp-conventions` agent skill. The local `SKILL.md` stubs in `.opencode/skills` and `.dsh/skills` point here, so the skill content is maintained in this file only. It is reference material for AI agents; the contributor-facing rules live in [CONTRIBUTING.md](../../CONTRIBUTING.md).

This project enforces many style rules via `.editorconfig` as build errors, so the compiler catches those automatically. The rules below are split into two groups: skip re-checking the first group (the build already fails if violated), and focus review effort on the second group (a linter cannot evaluate these, only judgment can).

## Group 1: build-enforced (do not spend review time re-checking these, but you must still write new code following them)

- Never use `var` (error); use explicit types + target-typed `new()`.
- File-scoped namespaces, one type per file, no primary constructors, no top-level statements.
- No expression-bodied methods/constructors (error); single-line property getters only.
- No braces around single-statement `if`/`for` (error); braces always on a new line.
- Use collection expression (`[]`) syntax for collections.
- Explicit `using (...) {}` blocks only, no `using` declarations (error).
- `is null` for null checks.
- Naming: `_camelCase` private readonly fields, `s_` static fields, `ALL_CAPS` constants, `I`-prefixed interfaces.
- Method parameters go on a single line, unless they substantially exceed a 1920px wide monitor's edge, in which case they wrap.
- Records always wrap their parameters: opening parenthesis on the same line as the record name, closing parenthesis on its own line, unindented.
- Usings wrapped in a `#region ========================================================================= USING =====================================================================================` block; 4-space indent, LF line endings.
- Enum members have documentation with an empty line between them. Enum summaries start with "Enumeration for ".
- Classes order members: fields first, then read-only fields, then properties, then constructors, then methods.

## Group 2: judgment-based (this is what review effort should focus on, a linter cannot check these)

- **Naming quality**: names must be explicit, even if long. Domain names must match the project's Ubiquitous Language exactly. Lambda parameters must be named descriptively, never single letters or shortened anti-pattern names (no `x`, `e`, `s`, etc).
- **Domain nullability**: domain code must never use nullable value types or reference nulls directly, it must use the `Optional<T>` and `Result<T>` types instead. Flag any raw `null`/`?` usage in domain code.
- **Comments**: should explain *why*, not *what* (the code already shows what). Wrap only when substantially exceeding a 1920px monitor's width. Flag comments that just restate the code.
- **XML documentation**: required on every public construct, and also on private properties and methods. Use `<see langword="etc"/>` for language keywords, not `<c>etc</c>`. Method summaries must not expose internal implementation details, unless truly relevant, in which case put that detail in a `<remarks>` block instead of the summary.
- **`<inheritdoc/>`**: avoid on classes implementing interface members. Developers usually work in the implementing class and should see documentation there directly, without having to open the interface.
- **Boolean naming**: boolean fields, properties, and methods returning booleans must start with a verb implying true/false: `is`, `are`, `should`, `has`, `was`, `can`, etc.
- **Error handling**: rule violations return `Result<T>`. Never throw exceptions unless explicitly justified (e.g. startup/configuration code). Flag any `throw` outside that context.

## Review priority

When reviewing a `.cs` file, spend the majority of effort on Group 2 items, since the build will already reject Group 1 violations before the code can even merge. Only flag a Group 1 item if you suspect `.editorconfig` enforcement is somehow not covering it (e.g. a suppressed warning, a `#pragma` disabling the rule, or a file excluded from analysis).
