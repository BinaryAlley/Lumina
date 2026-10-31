# Project rules

These rules apply to every agent, on every task, in this repository. For C# code style specifics, see the `csharp-conventions` skill (loaded on-demand when writing or reviewing `.cs` files). For tests, see `csharp-test-conventions` skill.

## Architecture / wiring

- Layers: Domain (bounded contexts/aggregates under `Core`), Application (commands/queries/handlers/validators + domain event handlers), Infrastructure (library scan jobs, services), DataAccess (EF Core DbContext, repositories/UoW, migrations), Presentation.Api (FastEndpoints under `Core/Endpoints/...` mirroring Application queries), Presentation.Web (MVC, localized `{culture}/{*catchall}` routes, resx localization across ~10 cultures), Contracts (shared DTOs).
- Each project has ALWAYS two top level directories: `Common`, where things common to that project go (i.e. DI, Models, Errors, etc), and `Core`, where things that make up the central part of that project reside.
- A `Common/Utilities` folder stores extension methods ONLY: every class in it must be a static class whose members are extension methods that take a `this` receiver. A helper that is not an extension method, such as a static value format checker or a constants holder, goes in the `Common` folder that describes its role (for example `Common/Playback`), never in `Utilities`.
- API contracts have their own dedicated project.
- As specified by DDD, objects should not be directly referenced cross-bounded contexts, instead, only Id objects placed in `ExternalIdentifiers` directories should be used. This loose coupling will greatly help a possible future migration to a microservices architecture.
- Composition root: each app's `Program.cs` registers all layers via `Add<Layer>LayerServices()` DI extensions in each project's `Common/DependencyInjection/`.
- Request flow: FastEndpoints endpoint -> mapping -> validator -> handler -> domain -> repository/UoW -> EF Core SQLite. Domain events fire via `EventualConsistencyMiddleware` (excluded for the `/scanProgressHub` SignalR hub).
- Handlers and domain event handlers are resolved directly from the DI container; commands/queries/validators/handlers are registered by assembly scan in `ApplicationLayerServices`.
- API contracts should be as much as possible records. Database entities should have as many properties as `required` and `init`. Domain properties should all be `private set`.
- DataAccess persistence: repositories persist aggregate graphs only through the closed toolkit in `Lumina.DataAccess\Common\Persistence` (`EditableValuesCopier`, `CollectionReconciler`, `SharedReferenceResolver`). Those primitives must stay generic and entity-agnostic. Never add a per-entity helper to that folder, and never hand-roll an equivalent in a repository: generalize the primitive (generics plus a marker interface such as `ISharedReferenceEntity`) or keep the logic in the repository.

- Presentation layer: immediately decouples API contracts with manual mapping: ToQuery or ToCommand. FastEndpoints adds authentication by default, only `AllowAnonymous` is free access. Errors are always `ProblemDetails`.
- Application layer: mapping classes are extension methods in `Common\Mapping`, and each mapping file should *only* extend the type the name of the mapping file uses, ex: `UserEntityMapping` contains *only* `UserEntity` extension methods to map to other types. The used names are: `ToQuery`, `ToCommand`, `ToRepositoryEntity`, `ToRequest`, `ToResponse`, `ToDto`, `ToDomainEntity`, `ToDomainValueObject`. Any deviation needed is confirmed with the user. Validators validate application concerns: null or empty strings, negative numbers, correct emails, etc, they do not do async work and they do not do business rules validation. They are the first thing invoked in handlers. The next check should be for authentication/authorization.
- Domain layer: no nullable types or exceptions thrown. Use `Optional<T>` and `Result<T>`. Validations resume to *business rules* validations and invariants, not application validation.
- If a class or file is located or does things in a way that has no precedent in the code base, VALIDATE WITH THE USER FIRST! DO NOT introduce new patterns, styles and conventions without getting permission first! 

## Documentation

- `docs/technical/requests` mirrors the folder tree of `src/Lumina.Presentation.Api/Core/Endpoints` down to the operation groups, in kebab-case, collapsing the per-operation leaf folders, and holds one `<Verb><Entity>.http` file per endpoint.
- `docs/technical/api` holds one `Api.<Resource>.md` file per `ApiRoutes.<Resource>` class, grouped in the endpoint sub-area folder rather than in a folder per resource, so a sub-area folder holds all the API docs of that sub-area.
- ADRs under `docs/technical/architecture/architecture-knowledge-management/architecture-decision-log` are append only. An existing ADR is never rewritten: its headings, paragraphs, tables and amendments stay exactly as they are. A new decision is recorded by appending a `## Amendment: <Title> (<date>)` section at the very end of the relevant ADR, after every existing amendment and in chronological order. Amendments are never written in the middle of an ADR, and an amendment is never removed, merged, relocated or reordered. A genuinely new decision gets a new ADR file.
- Every ADR uses ADR-0001 as the template for its structure and sections: a `# Architecture Decision Record (ADR): <number> - <title>` heading, `## Status`, `## Context`, `## Decision`, `## Consequences` with `### Positive Outcomes` and `### Risks and Tradeoffs`, `## Alternatives Considered`, and then the amendments. New ADRs must be created in that shape.
- Add every new documentation file to `src/Lumina.slnx` as a solution item under the matching solution folder, in the same change.
- The user facing manual lives in `docs/manual`. It describes Lumina as it is, not as it is planned. Whenever a change makes the manual drift from reality (features added or removed, defaults, ports, supported media types, bundled plugins or themes, configuration options, or wording that no longer matches the product), update the manual in the same change.
- Every manual page opens with a `## Contents` list of all manual sections, in reading order, with the current page marked in bold and every other page linked by relative path. When a section is added, renamed, reordered or removed, update the `## Contents` list on every manual page in the same change.

## Completing the work

- Never deliver an incomplete job while presenting it as done. A task is only done when every part of it is implemented, wired up, and verified to the same standard as the rest of the repository.
- If a task is genuinely larger than what can be completed in a single pass, do not silently do a fraction of it and call it finished. Choose one of these, explicitly:
  - Delegate to subagents, one per subtask or workstream, and act as the coordinator/orchestrator: keep your own context for planning, dispatch, integration and verification, and let the subagents produce the bulk of the work.
  - Or, if you still do it yourself, state up front what will and will not be covered in this pass.
- Whenever anything is left unfinished, shortened, stubbed, simplified, or handled "pragmatically", say so plainly and completely: what was skipped, what corners were cut, what is not wired up, what was not verified, and what still has to be done. Put this in the final message to the user, and in the code or the docs when it is durable.
- Never claim or imply that something is complete, working, or verified when it is not. The user must never have to audit the repository afterwards to discover what is actually done and what is not.

## Global output rules

- Commit messages: third person singular.
- In all generated text: no em-dashes, en-dashes, or emojis. Use `-`, `,`, `:`, or split the sentence instead.
- When writing or reviewing `.cs` files, load the `csharp-conventions` skill first - do not rely on memory for C# style rules, the skill is the source of truth and gets updated independently of this file. When writing tests, load the `csharp-test-conventions` skill first.
