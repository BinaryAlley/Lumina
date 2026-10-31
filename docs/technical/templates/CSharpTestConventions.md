---
name: csharp-test-conventions
description: Exact conventions and reference templates for writing C# test classes and fixtures in this project - test project structure (Unit/Integration/Security), file/namespace mirroring rules, fixture pattern, and per-layer test templates for endpoints, validators, handlers, repositories, integration, and security tests. Load this before writing or reviewing any test file (*Tests.cs) or fixture file (*Fixture.cs).
---

> This file is the canonical, tracked copy of the `csharp-test-conventions` agent skill. The local `SKILL.md` stubs in `.opencode/skills` and `.dsh/skills` point here, so the skill content is maintained in this file only. It is reference material for AI agents; the contributor-facing rules live in [CONTRIBUTING.md](../../CONTRIBUTING.md).

# Test project structure

There are three parallel test trees, each with its own top-level directory and its own set of per-source-project test projects:

```
tests/
├── Common/
│   ├── Lumina.Domain.Fixtures/
│   ├── Lumina.Application.Fixtures/
│   ├── Lumina.DataAccess.Fixtures/
│   ├── Lumina.Contracts.Fixtures/
│   └── Lumina.Presentation.Api.Fixtures/    
│   └── ... (created on demand, one per source project that has fixture-needing types)
├── UnitTests/
│   ├── Lumina.Domain.UnitTests/
│   ├── Lumina.Application.UnitTests/
│   ├── Lumina.Presentation.Api.UnitTests/
│   └── ... (one per source project that has unit tests)
├── IntegrationTests/
│   ├── Lumina.Presentation.Api.IntegrationTests/
│   └── ... (one per source project that has integration tests)
└── SecurityTests/
    ├── Lumina.Presentation.Api.SecurityTests/
    └── ... (one per source project exposed at an HTTP boundary)
```

Rules:

- Each test project's internal directory structure **mirrors the corresponding source project's directory structure exactly**. A class at `Lumina.Application/Core/Authentication/Login/LoginUserQueryValidator.cs` gets its unit test at `tests/UnitTests/Lumina.Application.UnitTests/Core/Authentication/Login/LoginUserQueryValidatorTests.cs` - same relative path, different project root.
- Test project naming is always `<SourceProjectName>.<Category>Tests` (e.g. `Lumina.Domain.UnitTests`, `Lumina.Presentation.Api.IntegrationTests`, `Lumina.Presentation.Api.SecurityTests`). Fixture project naming is always `<SourceProjectName>.Fixtures` (no category suffix - a fixtures project is not owned by any one test category, it's referenced by all of them as needed).
- The test class name is always `<TestedClassName>Tests`, identical across all three trees when the same source class is tested from multiple angles - e.g. `LoginEndpointTests` legitimately exists in both `Lumina.Presentation.Api.UnitTests` (mocked handler) and `Lumina.Presentation.Api.IntegrationTests` (real pipeline) and `Lumina.Presentation.Api.SecurityTests` (security-focused HTTP tests), because they live in different projects/namespaces. This is not duplication, it is three different concerns about the same endpoint.
- Only source projects with an HTTP-facing boundary (Presentation.Api today) need a `SecurityTests` project. Domain/Application/DataAccess projects generally only need `UnitTests`, since there's no external attack surface to test at that layer directly. Confirm with the developer before creating a `SecurityTests` project for a project type not already established.

# Fixture location: always centralized, no exceptions

There is exactly one rule, with no decision to make and no branching: **every fixture lives in `tests/Common/<SourceProjectName>.Fixtures/`, mirroring that type's own path inside its source project, with no extra `Fixtures` subdirectory inserted at each level** (the project itself is already dedicated to fixtures, so repeating that folder name would be redundant). This applies uniformly whether the type is:

- consumed by a single test project (e.g. a Domain aggregate only ever tested by `Lumina.Domain.UnitTests`),
- consumed by many test projects across trees (e.g. a Contract DTO used by `UnitTests`, `IntegrationTests`, and `SecurityTests`),
- or consumed by projects in a different source layer entirely (e.g. an EF entity defined in `Lumina.Application` but used by `Lumina.DataAccess.UnitTests` too).

There is no "local vs shared" distinction anymore. Never create a `Fixtures` folder inside a `*Tests` project. If you ever see one from before this rule existed, treat it as tech debt: the fixtures inside it should be moved to the matching `tests/Common/<SourceProject>.Fixtures/` project, and the consuming test project should take a project reference to it instead - flag this in your report rather than doing a large migration unprompted, but do not add anything new to a local `Fixtures` folder going forward.

```
Source:  Lumina.Domain/Core/BoundedContexts/WrittenContentLibraryBoundedContext/BookLibraryAggregate/Book.cs
Fixture: tests/Common/Lumina.Domain.Fixtures/Core/BoundedContexts/WrittenContentLibraryBoundedContext/BookLibraryAggregate/BookFixture.cs

Source:  Lumina.Application/Common/DataAccess/Entities/Authorization/PermissionEntity.cs
Fixture: tests/Common/Lumina.Application.Fixtures/Common/DataAccess/Entities/Authorization/PermissionEntityFixture.cs

Source:  Lumina.Contracts/Common/Models/Contracts/Requests/LoginRequest.cs
Fixture: tests/Common/Lumina.Contracts.Fixtures/Common/Models/Contracts/Requests/LoginRequestFixture.cs
```

Any test project, in any tree (Unit/Integration/Security), takes a normal project reference to whichever `<SourceProject>.Fixtures` project(s) it needs - never a reference to another `*Tests` project. Before creating any new fixture, grep the whole `tests/Common/` tree for `<TypeName>Fixture` to check whether it already exists. If a fixtures project for a given source project doesn't exist yet, create it (matching naming/structure above) rather than defaulting to placing the fixture somewhere else - ask the developer only if it's genuinely unclear which source project a type belongs to.

# Location and naming rules

These are non-negotiable structural rules, checked before writing a single line of test code.

- **Test classes mirror tested classes exactly.** Same name with `Tests` appended, same namespace, same hierarchical directory path. Example:
  - Source: `Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Book.cs`
  - Test: `Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.BookTests.cs`
- **Fixtures always mirror their source project's own path exactly**, inside `tests/Common/<SourceProject>.Fixtures/`, per the section above.
- One test class per tested class, one fixture per model/DTO/entity/query/command that needs one. Do not combine multiple types' tests or fixtures into one file.

# Fixtures are always used through an instance - never called statically

This is a hard rule with no exceptions: fixtures are instantiated (`private readonly XFixture _xFixture = new();`) and used via `_xFixture.Create(...)`, never invoked as a static method (`XFixture.Create(...)`). This applies to every fixture type: entities, DTOs, requests, queries, commands, everything.

Reasons this matters, not just style:
- **Updatability for all consumers**: an instance-based fixture can carry internal state (like an `idCounter`, incrementing an Id property - but per fixture instance, not global!) and compose with other instance fixtures (e.g. a `LibraryEntityFixture` holding its own `_libraryScanEntityFixture` instance) consistently across every test that uses it. A static method can't participate in that composition.
- **Dual-mode generation stays consistent**: the whole point of the fixture pattern is "call it with nothing and get valid random data, or call it with explicit overrides including `null`." That contract must be uniform everywhere a given type is created in tests, and instance methods make that contract impossible to accidentally bypass.
**Always named `Create()`** (or `CreateMany(count)` for a batch variant). Never a type-specific method name like `CreateRequestBook()` or `CreateLoginQuery()`.

If you encounter an existing fixture in the codebase that's called statically, treat it as a bug to flag (for `quality-reviewer`/`tester`) rather than a pattern to replicate, and prefer converting it to instance-based when you touch it.

# Tooling stack (use these, not alternatives)

- **Test framework**: xUnit (`[Fact]`, `[Theory]`, `[InlineData]`).
- **Mocking**: NSubstitute (`Substitute.For<T>()`, `Arg.Any<T>()`, `Arg.Is<T>(...)`, `.Returns(...)`, `.Received(n)`, `.DidNotReceive()`).
- **Fixture data generation**: AutoFixtue and Bogus (`Faker<T>()`, `.CustomInstantiator(...)`, `.Generate()`), wrapped inside a dedicated `*Fixture` class instantiated per test class, never inlined ad hoc in a test and never called statically.
- **EF Core repository tests**: mocked `DbContext` via `Create.MockedDbContextFor<TContext>()`, asserting through `ChangeTracker.Entries<T>()` or by re-querying the mocked context, not by mocking `DbSet` methods directly.
- **Validator tests**: use the `ValidatorTestExtensions` extension, combined with this project's custom `ShouldHaveValidationError` / `ShouldNotHaveValidationError` assertion extensions.
- **Endpoint tests**: FastEndpoints via this project's `Factory.Create<TEndpoint>(...)` helper, asserting on the returned `IResult` type (`Ok<T>`, `ProblemHttpResult`, etc.) via `Assert.IsType<T>`.
- **Integration tests**: real HTTP pipeline via a `WebApplicationFactory`-based fixture (`AuthenticatedLuminaApiFactory` for pre-authenticated scenarios), real `HttpClient`, real DB access through a `IServiceScope` for setup/teardown, asserting on actual `HttpResponseMessage` and deserialized `ProblemDetails`.
- **Security tests**: same HTTP pipeline via `LuminaApiFactory` (not pre-authenticated), asserting on security-specific properties: response headers, rate limiting, timing, token structure, injection resistance, and information leakage.

# Test method naming - depends on how the test invokes the code

This is the single most important naming distinction in this skill, because the two rules look similar but come from a different source of truth.

**Unit tests call a C# method directly** (`_sut.ExecuteAsync(...)`, `_sut.HandleAsync(...)`, `_validator.Validate(...)`, `_sut.InsertAsync(...)`). Name after the literal method being invoked:

`MethodName_WhenCondition_ShouldAssertion` - e.g. `ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLoginResponse`, `HandleAsync_WhenUserDoesNotExist_ShouldReturnError`, `Validate_WhenPasswordIsNull_ShouldHaveValidationError`.

**Integration and security tests call an HTTP endpoint, not a C# method.** There is no meaningful "method under test" from the caller's perspective - `ExecuteAsync` is FastEndpoints' internal override and is invisible to an HTTP client. Name after the **endpoint's action**, derived from the endpoint class name with the `Endpoint` suffix removed:

`Action_WhenCondition_ShouldAssertion` - e.g. `LoginEndpoint` -> action `Login` -> `Login_WithValidCredentials_ShouldAuthenticateSuccessfully`; `GetPermissionsEndpoint` -> action `GetPermissions` -> `GetPermissions_WhenUnauthorized_ShouldReturnUnauthorizedResult`.

Do not use `ExecuteAsync` as the action name in an integration or security test, even though the endpoint's method happens to be called that internally - it describes FastEndpoints plumbing, not what the test is verifying from the outside. If you see `ExecuteAsync_...` naming inside an existing integration or security test file, treat it as the inconsistency to avoid, not a pattern to copy.

# Universal structural rules for every test/fixture class

- `[ExcludeFromCodeCoverage]` attribute on every test class and every fixture class.
- XML doc summary on the test class itself. For unit/integration tests targeting one class: `/// <summary>\n/// Contains unit tests for the <see cref="TargetType"/> class.\n/// </summary>` (swap "unit" for "integration" as appropriate). Security tests use the same class-targeting form: `/// <summary>\n/// Contains security tests for the <see cref="XEndpoint"/> class.\n/// </summary>`, matching every established security test file in the repo. Do not invent a route-based (`<c>/api/v1/route/path</c>`) summary form - if a new shape seems warranted, surface it for manual review instead of introducing it silently.
- XML doc summary on the constructor: `/// <summary>\n/// Initializes a new instance of the <see cref="TestClassName"/> class.\n/// </summary>`.
- System Under Test is always named `_sut` (unit tests only; integration/security tests use `_client` since they go through HTTP, not a direct SUT reference).
- Mocked dependencies are named `_mock<DependencyName>` (e.g. `_mockUserRepository`, `_mockHashService`).
- Fixture instances are named `_<typeName>Fixture` (e.g. `_loginRequestFixture`), declared `private readonly`, instantiated in the constructor or as a field initializer, always used via `.Create(...)`.
- All setup happens in the constructor (unit/security tests) or `InitializeAsync` (integration tests using `IAsyncLifetime`), never inside `[Fact]` methods.
- Every test body uses `// Arrange`, `// Act`, `// Assert` comments, in that order, with a blank line separating each section. `// Arrange` is required only when there is actual setup: a test whose action is a static factory call or an inline construction (for example `Mood.Create("  calm  ")` or `new RegistrationRequest(...)`) legitimately starts at `// Act`. Do not flag a missing `// Arrange` when no arrange step exists, and do not add an empty one to satisfy the pattern.
- Test method naming follows the invocation-based rule above - literal method name for unit tests, endpoint action name for integration/security tests. Never mix the two within the same file.
- Use `[Theory]` + `[InlineData]` for the same assertion across multiple input variations, with a trailing `//` comment on each `InlineData` line explaining what that case represents when it's not obvious from the value alone.
- Any default stub set up in a constructor for a dependency (e.g. a validator stubbed to pass) must have at least one dedicated test overriding it to exercise the opposite path, verifying both the resulting outcome and that no unwanted downstream side effect occurred.

# Fixture template (reference exemplar - match this structure exactly)

```csharp
/// <summary>
/// Fixture class for the <see cref="LibraryEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryEntityFixture
{
    private int idCounter = 1;
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly LibrarySnapshotEntityFixture _librarySnapshotEntityFixture = new();

    /// <summary>
    /// Creates a new <see cref="LibraryEntity"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional database Id.</param>
    /// <param name="name">Optional libary name.</param>
    /// <param name="libraryScanEntity">Optional library scan associated with this library.</param>
    /// <param name="librarySnapshotEntity">Optional library snapshot associated with this library.</param>
    /// <param name="includeLibaryScan">Whether to generate library scan or not.</param>
    /// <param name="includeLibrarySnapshot">Whether to generate library snapshot or not.</param>
    /// <returns>A configured <see cref="LibraryEntity"/> instance.</returns>
    public LibraryEntity Create(
        int? id = null,
        string? name = null,
		LibraryScanEntity? libraryScanEntity = null,
		LibrarySnapshotEntityFixture? librarySnapshotEntity = null,
		bool includeLibaryScan = false,
        bool includeLibrarySnapshot = false)
    {
        return new Faker<LibraryEntity>()
            .CustomInstantiator(f => new LibraryEntity
            {
                Id = id ?? idCounter++,
                Name = name ?? f.Commerce.Department(),
                LibraryScan = includeLibaryScan ? (libraryScanEntity ?? _libraryScanEntityFixture.Create()) : null,
                LibrarySnapshot = includeLibrarySnapshot ? (librarySnapshotEntity ?? _librarySnapshotEntityFixture.Create()) : null
            })
            .Generate();
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryEntity"/> instances.</returns>
    public List<LibraryEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
```

Every optional parameter defaults to `null` (or a sensible default for `bool`/`int`) and falls back to an Autofixture/Bogus-generated value when not supplied, so callers can request fully random data OR pin specific fields. A `??` fallback alone cannot express an explicit `null`: give every optional member its own `include<Property>` boolean flag (defaulting to `true`) so a caller can force that member to `null` from the start, rather than building a value only to immediately rewrite it via `with { ... }`. This dual mode is mandatory: never build a fixture that only supports full randomization or only supports explicit values. This holds true for every fixture type, including query/command fixtures used in validator and handler tests, not just entity/DTO fixtures - all of them are consumed by instantiating the fixture class and calling its instance method.

Every `include<Property>` flag goes after all value parameters, grouped together at the end of the parameter list, in the same order as the properties they guard. Never interleave an `include<Property>` flag between value parameters, and never put a value parameter after an `include<Property>` flag.

# Random value generation in fixtures is always bounded

Every random value produced in a fixture (AutoFixture, Bogus, `Random`) must come from an explicit, finite range. Never satisfy a predicate by searching an unbounded or lazily-infinite sequence. In particular, never do `_fixture.Create<Generator<int>>().First(predicate)`: AutoFixture draws each enumerated element fresh across the whole domain, so a narrow predicate (for example a year window of 126 values out of ~4 billion) can take an effectively unbounded number of draws. This periodically hung the CI test run for over 10 minutes with no failing test (the `BookFixture` `reReleaseYear` filtering was the culprit), on randomly varying operating systems. The same hazard applies to `Generator<T>` and similar infinite sequences used with `.First`, `.FirstOrDefault`, `.Where(...).First`, `.Last`, `.Single`, or `.TakeWhile`.

Rules:

- Draw from a bounded range in a single step: `Random.Shared.Next(1900, 2026)`, `faker.Random.Int(min, max)`, or `faker.Random.Number(min, max)`. Never filter a full-domain generator to reach that range.
- When two generated values share an invariant (for example a re-release year that must be at least the original year), derive the second value from the first while keeping every draw bounded: `originalYear = Random.Shared.Next(1900, 2026)` followed by `reReleaseYear = Random.Shared.Next(originalYear, 2026)`. If the caller can override the first value, never let the derived `Next` range invert: pick an exclusive upper bound that is always greater than the lower one, for example `Random.Shared.Next(resolvedOriginalYear, Math.Max(resolvedOriginalYear + 1, maximumYear))`. Never fall back to a `Generator<T>`-based search to work around an invalid range.
- If a bounded draw cannot express the valid domain directly, model the valid domain explicitly (a finite list or `Enumerable.Range`) and select from it.

# Layer templates

Identify which layer the class under test belongs to, then follow that layer's template exactly, only substituting the concrete types.

## Presentation layer: endpoint tests

Reference exemplar - a `LoginEndpointTests` class covering: success path returning the right `IResult` type, handler-returns-error mapping to `ProblemHttpResult` with correct status code/type/`ContentType`/error payload shape, confirming the correct query/command is sent to the handler with correctly mapped fields (via `Arg.Is<T>(q => ...)`), and cancellation behavior. Structure:

```csharp
[ExcludeFromCodeCoverage]
public class LoginEndpointTests
{
    private readonly IQueryHandler<LoginUserQuery, Result<LoginResponse>> _mockHandler;
    private readonly LoginEndpoint _sut;
    private readonly LoginRequestFixture _loginRequestFixture;

    public LoginEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<LoginUserQuery, Result<LoginResponse>>>();
        _sut = Factory.Create<LoginEndpoint>(_mockHandler);
        _loginRequestFixture = new LoginRequestFixture();
    }

	[Fact]
	public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLoginResponse()
	{
		// Arrange
		LoginRequest request = _loginRequestFixture.Create();
		CancellationToken cancellationToken = CancellationToken.None;
		LoginResponse expectedResponse = new(Guid.NewGuid(), "testUser", "jwt_token", true);
		_mockHandler.HandleAsync(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
		  .Returns(Result.From(expectedResponse));

		// Act
		IResult result = await _sut.ExecuteAsync(request, cancellationToken);

		// Assert
		Ok<LoginResponse> okResult = Assert.IsType<Ok<LoginResponse>>(result);
		Assert.Equal(expectedResponse, okResult.Value);
	}

	[Fact]
	public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
	{
		// Arrange
		LoginRequest request = _loginRequestFixture.Create();
		CancellationToken cancellationToken = CancellationToken.None;
		Error expectedError = Error.Validation("Login.Failed", "Invalid username or password.");
		_mockHandler.HandleAsync(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
		  .Returns(expectedError);

		// Act
		IResult result = await _sut.ExecuteAsync(request, cancellationToken);

		// Assert
		ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
		Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
		Assert.Equal("application/problem+json", problemDetails.ContentType);
		HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
		Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
		Assert.Equal("General.Validation", validationProblemDetails.Title);
		Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
		Assert.Single(validationProblemDetails.Errors);
		Assert.Equal(new[] { "Invalid username or password." }, validationProblemDetails.Errors["Login.Failed"]);
	}

	[Fact]
	public async Task ExecuteAsync_WhenCalled_ShouldSendLoginUserQueryToSender()
	{
		// Arrange
		LoginRequest request = _loginRequestFixture.Create();
		CancellationToken cancellationToken = CancellationToken.None;
		_mockHandler.HandleAsync(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
		  .Returns(Result.From(new LoginResponse(Guid.NewGuid(), "testUser", "jwt_token", true)));

		// Act
		await _sut.ExecuteAsync(request, cancellationToken);

		// Assert
		await _mockHandler.Received(1).HandleAsync(
			Arg.Is<LoginUserQuery>(q =>
				q.Username == request.Username &&
				q.Password == request.Password &&
				q.TotpCode == request.TotpCode),
			Arg.Is(cancellationToken));
	}

	[Fact]
	public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
	{
		// Arrange
		LoginRequest request = _loginRequestFixture.Create();
		CancellationTokenSource cts = new();
		TaskCompletionSource<bool> operationStarted = new();
		TaskCompletionSource<bool> cancellationRequested = new();

		_mockHandler.HandleAsync(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Task.Run(async () =>
			{
				operationStarted.SetResult(true);
				await cancellationRequested.Task;
				callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
				return Result.From(new LoginResponse(Guid.NewGuid(), "testUser", "jwt_token", true));
			}, callInfo.Arg<CancellationToken>()));

		// Act
		Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
		await operationStarted.Task;
		cts.Cancel();
		cancellationRequested.SetResult(true);

		// Assert
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
	}
}
```

Minimum coverage for an endpoint test class: success mapping, error mapping (with full problem-details shape assertions), request-to-query/command field mapping verification, and cancellation propagation if the endpoint is async. Note `_loginRequestFixture` is instantiated (`new LoginRequestFixture()`) in the constructor and used via `.Create()` - the same instance pattern applies to request/query/command fixtures as to entity fixtures.

## Application layer: validator tests

Reference exemplar - a `LoginUserQueryValidatorTests` class. Structure: constructor builds a bare `_validator = new Validator()` (validators typically have no dependencies to mock), plus a `private readonly LoginUserQueryFixture _loginUserQueryFixture = new();` field for building the query under test. Every validated field gets its own `[Fact]`(s) for null/empty/whitespace/invalid-format cases using `ShouldHaveValidationError`, plus at least one `[Theory]`/`[InlineData]` block for pattern-based fields (passwords, codes) covering both invalid and valid values with `ShouldHaveValidationError`/`ShouldNotHaveValidationError` respectively, and a final `Validate_WhenQueryIsValid_ShouldNotHaveValidationError` happy-path test asserting no errors across all fields at once. Always build the base valid object from the instantiated query/command fixture (`_loginUserQueryFixture.Create(...)`, never a static call). Prefer expressing the field under test through the fixture's own boolean `include<Property>` flags, so the base object is correct from the start and each test only varies one dimension. Use the `with { Field = ... }` record workaround only when the fixture cannot produce the needed variation (for example a nested DTO shape it exposes no flag for), never as the default.

## Application layer: handler tests

Reference exemplar - a `LoginUserQueryHandlerTests` class. Constructor mocks every constructor dependency individually (`Substitute.For<IDependency>()`), wires cross-dependencies (e.g. `_mockUnitOfWork.UserRepository.Returns(_mockUserRepository)`), stubs a passing validator by default (`mockValidator.Validate(Arg.Any<T>()).Returns([])`) so that every test not specifically about validation doesn't need to repeat that setup, instantiates any needed fixtures (`private readonly UserEntityFixture _userEntityFixture = new();`), then constructs `_sut` with all mocks.

**The validator-passes stub is a default for convenience, not a substitute for testing validation failure.** Every handler test class must also include at least one dedicated test that overrides the constructor default to make the mock validator return one or more errors (e.g. `mockValidator.Validate(Arg.Any<T>()).Returns([someError])`), then asserts:
1. The handler returns that failure as its `Result<T>`/`ErrorOr<T>` outcome (short-circuits correctly instead of proceeding).
2. No downstream dependency was invoked as a result (e.g. `await _mockUserRepository.DidNotReceive().GetByUsernameAsync(...)`, `await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(...)`) - confirming the handler actually stops at validation and doesn't waste work or cause side effects on invalid input.

Beyond that, each test sets up the specific mock return values needed for its scenario, calls `HandleAsync`, and asserts on the returned `Result<T>`/`ErrorOr<T>` (`IsFailure`, `.Value`, `.FirstError`), plus verifies side-effect calls where relevant (`Received(1)`, `DidNotReceive()`) especially for state-changing paths (repository updates, `SaveChangesAsync`). Cover: the full happy path (including any conditional branches like optional TOTP), every distinct failure branch with its own specific expected error (including the validator-failure branch above), and verification that failure paths do NOT trigger persistence side effects.

## DataAccess layer: repository tests

Reference exemplar - a `UserRepositoryTests` class. Constructor creates `_mockContext = Create.MockedDbContextFor<LuminaDbContext>()` and `_sut = new Repository(_mockContext)`, plus instantiated fixtures (`private readonly UserEntityFixture _userEntityFixture = new();`). Tests interact with the real (mocked) `DbContext` behavior: seed data directly via `_mockContext.Entities.Add(...)` + `SaveChangesAsync()` when a pre-existing-state scenario is needed, then call the repository method under test and assert both the `Result` outcome AND the actual `ChangeTracker` state (e.g. `EntityState.Added`, entry count) to confirm the repository did what it claims.

# Integration test template

Live under `tests/IntegrationTests/<SourceProject>.IntegrationTests/`. Test the full real pipeline against a pre-authenticated context, over HTTP - so naming uses the **endpoint action**, not `ExecuteAsync`.

```csharp
[ExcludeFromCodeCoverage]
public class LoginEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly string _testUsername;

    public LoginEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    public async Task InitializeAsync() { _client = _apiFactory.CreateClient(); await Task.CompletedTask; }

    [Fact]
    public async Task Login_WhenCalledWithValidCredentials_ShouldLoginSuccessfully()
    {
        // Arrange
        UserEntity user = await CreateTestUser();
        LoginRequest request = new(Username: user.Username, Password: "TestPass123!");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null) { dbContext.Users.Remove(user); await dbContext.SaveChangesAsync(); }
    }
}
```

Key characteristics: `IClassFixture<AuthenticatedLuminaApiFactory>` + `IAsyncLifetime` (not plain `IDisposable`), a fresh uniquely-named test user created and torn down per test, real HTTP calls, action-based naming (`Login_...`, never `ExecuteAsync_...`), and assertions on the deserialized `ProblemDetails` shape (`status`, `type`, `title`, `detail`, `instance`, `traceId`) for every non-success response, matching the exact RFC section URLs used by this project's error mapping. Cover: full happy path per distinct valid credential type (password, password+TOTP, temp password), and every distinct failure mode with its exact expected `ProblemDetails` shape (expired temp password, invalid credentials, invalid TOTP).

# Security test template

Live under `tests/SecurityTests/<SourceProject>.SecurityTests/`. Test the security properties of a route, independent of whether the business logic itself is correct - also over HTTP, so also action-based naming.

```csharp
[ExcludeFromCodeCoverage]
public class LoginEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly string _testUsername;

    public LoginEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.1");
        _testUsername = $"testuser_{Guid.NewGuid()}";
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Users--")] // destructive injection
    public async Task Login_WithSQLInjectionAttempt_ShouldRemainSecure(string maliciousUsername)
    {
        // Arrange
        LoginRequest request = new(Username: maliciousUsername, Password: "Abcd123$");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
    }

    public void Dispose()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity? user = dbContext.Users.FirstOrDefault(u => u.Username == _testUsername);
        if (user is not null) { dbContext.Users.Remove(user); dbContext.SaveChanges(); }
    }
}
```

Key characteristics: `IClassFixture<LuminaApiFactory>` + plain `IDisposable` (sync, per-test cleanup since xUnit instantiates the class per `[Fact]`), a unique `X-Forwarded-For` IP per test to isolate rate-limiting/timing tests from each other, and action-based naming (`Login_WithX_ShouldY`), never `ExecuteAsync_WithX_ShouldY`.

Categories to cover for any authenticated/sensitive endpoint, unless clearly not applicable:

- **Unauthorized access**: calling without credentials returns 401 with the exact expected `ProblemDetails` shape.
- **Injection resistance**: `[Theory]`/`[InlineData]` with SQL/command/template injection payloads on every user-controlled input field, asserting the response never leaks `SQL`, `Exception`, stack traces, or other users' data, and returns an appropriately generic error rather than a 500.
- **Rate limiting**: exceeding the configured request limit returns 429 with `X-RateLimit-*` headers and the correct `retryAfter` value.
- **Timing attack resistance** (for auth-comparison endpoints specifically): statistical variance across repeated requests stays within a defined tolerance, so response time doesn't leak whether a username/credential exists.
- **Sensitive data non-leakage**: response bodies never contain `password`, `hash`, `salt`, or other secrets, even in error paths.
- **Token/claim correctness** (for endpoints issuing tokens): algorithm, issuer, audience, expiration window, and claim contents match the documented contract exactly.

# When no exact template fits

If the class/route under test doesn't clearly match one of the templates above, still apply the universal structural rules and naming conventions (choosing method-name vs. action-name based on whether the test invokes a C# method or an HTTP endpoint), and model the closest available existing test file in the same test project rather than inventing a new structure from scratch. Ask the developer before creating a new test project category that doesn't already exist in the `tests/` tree.
