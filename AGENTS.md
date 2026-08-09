# AGENTS.md

Lumina is a self-hosted media server: an API server (FastEndpoints + EF Core/SQLite) and a
web client (ASP.NET Core MVC + SignalR), written in Clean Architecture / DDD, targeting .NET 9.

## Build / test / lint
- Solution is `src/Lumina.sln` (run all dotnet commands from the repo root with that path).
- There is NO separate lint/format step. `dotnet build` IS the linter: `.editorconfig` marks
  style rules as `error` and every project sets `TreatWarningsAsErrors=true` (NU1901-1904 excluded).
- CI order (`run-tests.yml`): `dotnet restore src/Lumina.sln` -> `dotnet build src/Lumina.sln --no-restore`
  -> `dotnet test src/Lumina.sln --no-build`, on Ubuntu/Windows/macOS for PRs to `master`.
- Focused test: `dotnet test tests/UnitTests/Lumina.Domain.UnitTests` or add
  `--filter FullyQualifiedName~...` to the solution-wide run. Some file-system suites have
  historically flaky tests (parallelization is disabled in several projects).

## C# conventions (enforced by .editorconfig as errors, build FAILURES if violated)
- Never use `var` (error); use explicit types + target-typed `new()`.
- File-scoped namespaces, one type per file, no primary constructors, no top-level statements.
- No expression-bodied methods/constructors (error); single-line property getters only.
- No braces around single-statement `if`/`for` (error); braces always on a new line.
- Use collection expression (`[]`) syntax for collections.
- Explicit `using (…) {}` blocks only, no `using` declarations (error).
- `is null` for null checks; `_camelCase` private readonly fields, `s_` static fields,
  ALL_CAPS constants, `I`-prefixed interfaces. Names must be explicit, even if they become long. Domain names must match the Ubiquitous Language. Lambda expression parameters must be named plainly, do not use "x" or any single letter shortcuts. Domain must *never* use nullable values, it should make use of the `Optional<T>` and `ErrorOr<T>` types that are available. 
- Method parameters go on a single line, unless they substantially go off screen edge on a 1920px wide monitor, when they should be wrapped.
- Records should ALWAYS wrap their parameters, and the opening parenthesis should be on the same line as the
  record name, while the closing parenthesis should be on its own line, unindented
- Comments should be added for relevant blocks of code and they shouldn't explain the "what" (the code already does that),
  they should explain the "why". They should be wrapped only when they substantially go off screen edge on a 1920px wide monitor.
- XML doc comments on every public (and also private properties and methods) construct; usings wrapped in a
  `#region ========= USING =========` block; 4-space indent, LF endings.
- Enum members have documentation, and an empty line between them. Enums have summaries that start with `Enumeration for `.  
- Commit messages in third person singular.
- In all the  generated text, no em-dashes, en-dashes, or emojis. Use `-`, `,`, `:`, or split the sentence.
- Lambda expressions parameters should be named properly and descriptive, do not use names shortened to a single letter, or other common anti patterns.
- Rule violations return `ErrorOr<T>` - never throw exceptions unless explicitly needed, for example, in startup configuration files.
- In XML documentation, use `<see langword="etc"/>` for language specific references, don't use the generic `<c>etc</c>`. Documentation for methods should
  NOT expose internal implementation (don't explain how a method does things in its summary, unless it is really relevant, and if it is, put it in <remarks> blocks).
- prefer avoiding `/// <inheritdoc/>` on classes implementing interface members, usually users work in the implementing class and they should
  see the documentation without having to dig to the interface all the time.
- Boolean members (fields, properties, methods returning boolean values) should always start with a verb indicating a true or false output: is, are, should, has, was, can, etc.
- Classes should *always* have fields at the top, then read-only fields underneath, then properties, then constructors, then methods.

## Branching (CI-enforced)
- Branch names MUST start with `feature/`, `bugfix/`, `hotfix/`, `documentation/`, `refactor/`, or `other/`.
- PRs target `master`; the validate workflows reject other branch names.

## Architecture / wiring
- Layers: Domain (bounded contexts/aggregates under `Core`), Application (Mediator commands/queries/
  handlers/validators + domain event handlers), Infrastructure (library scan jobs, services),
  DataAccess (EF Core DbContext, repositories/UoW, migrations), Presentation.Api (FastEndpoints under
  `Core/Endpoints/...` mirroring Application queries), Presentation.Web (MVC, localized
  `{culture}/{*catchall}` routes, resx localization across ~10 cultures), Contracts (shared DTOs).
- Each project has ALWAYS two top level directories: `Common`, where things common to that project go (ie: DI, Models, Errors, etc), and `Core`,
  where things that make up the central part of that project reside.
- API contracts have their own dedicated project.
- As specified by DDD, objects should not be directly referenced cross-bounded contexts, instead, only Id objects placed in `ExternalIdentifiers`
  directories should be used. This loose coupling will greatly help a possible future migration to a micro services architecture.
- Composition root: each app's `Program.cs` registers all layers via `Add<Layer>LayerServices()`
  DI extensions in each project's `Common/DependencyInjection/`.
- Request flow: FastEndpoints endpoint -> Mediator handler -> repository/UoW -> EF Core SQLite.
  Domain events fire via `EventualConsistencyMiddleware` (excluded for the `/scanProgressHub` SignalR hub).
- Mediator is source-generated (`Mediator.SourceGenerator`).
- Requests and Responses API contracts should immediately be decoupled by internal workings via manual mappings done as extension methods (located in `Lumina.Application\Common\Mapping`
- API contracts should be as much as possible records. Database entities should have as many properties as `required` and `init`. Domain properties should all be `private set`.

## Data / EF Core
- SQLite `Lumina.db` is created next to the API output binary; `MigrateAsync()` runs on API startup.
- Schema changes require a new migration in `src/Lumina.DataAccess/Common/Migrations`
  (`dotnet ef migrations add ...`; EF Design is referenced in the API csproj).
- Entity/configs live in `src/Lumina.DataAccess/Common/Configuration`.

## Running locally
- API: `dotnet run --project src/Lumina.Presentation.Api` -> http://localhost:5214 (Scalar/OpenAPI UI).
- Web: `dotnet run --project src/Lumina.Presentation.Web` -> http://localhost:5012.
- API `CorsSettings:AllowedOrigins` must include the Web app's Kestrel URL or SignalR fails
  (docs/INSTALL.md); `ServerConfiguration:BaseAddress/Port` in Web appsettings points at the API.
- `appsettings.development.json` / `appsettings.test.json` are GITIGNORED (`*.[Dd]evelopment.json`,
  `*.[Tt]est.json`) and hold JWT/encryption dev secrets. Never commit them. Shared config lives in
  `src/Lumina.Infrastructure/appsettings.shared*.json`.
- Serilog `LOG_PATH` env var (defaults to `/logs` in containers, `bin/.../logs` locally).
- Stale: both `Dockerfile`s still use .NET 8 base images while projects target net9.0 — docker
  builds are likely broken until updated.
- Manual API testing: `docs/technical/requests/**/*.http` (REST Client; dev host http://localhost:5214).
- Behind a reverse proxy, forward `X-Forwarded-For` or IP-based rate limiting becomes global (docs/INSTALL.md).

## Tests
- Each tested class has an identically named test class (ie: `AddBookCommandHandler` - `AddBookCommandHandlerTests`), 
  and the paths must match precisely (the test class should follow the same directory structure inside the test project
  as the tested class has in its project. Fixture classes go in the same directory structure as the members they are
  mocking, with an additional `Fixtures/` subdirectory, and they have the same name as the class they mock, plus the `Fixture` suffix.
- `tests/UnitTests/<Project>.UnitTests`: xUnit + NSubstitute + AutoFixture + Bogus; fixtures in
  `Fixtures/` dirs; xUnit `Assert.*` only (FluentAssertions was removed); naming
  `MethodToTest_WhenCondition_ShouldAssertion`; allowed test libraries are a fixed list (docs/CONTRIBUTING.md).
- `tests/IntegrationTests/...`: `WebApplicationFactory` + in-memory SQLite (`LuminaApiFactory`,
  `AuthenticatedLuminaApiFactory`), require `appsettings.test.json` + shared configs.
- CI runs on 3 OSes: keep file-system tests platform-agnostic.
- Reference docs: docs/CONTRIBUTING.md (style/tests), docs/INSTALL.md, docs/technical/ (domain, ADRs, .http requests).
