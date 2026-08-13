# Architecture Decision Record (ADR): 0011 - Replace MediatR with Custom CQRS Abstractions

## Status

**Accepted** (2026-08-13)

## Context

The solution uses MediatR as a lightweight mediator to encapsulate command and query handling across the application layer. Following MediatR's shift to a commercial licensing model starting with .NET 8, continued use introduces an unacceptable external dependency on a paid third-party library for functionality that is entirely implementable in-house.

Key constraints that shaped this decision:

1. **Licensing Change**: MediatR became a commercial product for .NET 8+ projects, requiring a paid license for continued use in production systems.

2. **Narrow Usage Scope**: MediatR is used for command and query dispatching, with a single pipeline behavior (automatic application validation) and no stream requests. Its notification publishing is used for domain event publishing, which is outside the scope of this ADR and is replaced separately (see ADR-0013).

3. **Zero Abstraction Value at Scale**: With only a validation pipeline behavior registered, the command and query dispatching provides no runtime value beyond a dispatch indirection that can be replaced by direct handler injection. It actually hurts performance by runtime dispatching of handlers via reflection.

4. **Ownership and Transparency**: A custom implementation is fully owned, inspectable, and carries no transitive dependency risk or future licensing exposure.

## Decision

Replace MediatR with minimal, hand-written CQRS abstractions placed in `Lumina.Application.Common.CQRS`. Handlers are resolved directly via the .NET dependency injection container - no mediator object, no runtime reflection dispatch.

### Core Abstractions

```csharp
// src/Lumina.Application/Common/CQRS/ICommand.cs

/// <summary>
/// Marker interface for command types handled by <see cref="ICommandHandler{TCommand, TResult}"/>.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Defines a contract for handling commands in the application layer.
/// Commands represent operations that modify application state.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the command.</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
```

```csharp
// src/Lumina.Application/Common/CQRS/IQuery.cs

/// <summary>
/// Marker interface for query types handled by <see cref="IQueryHandler{TQuery, TResult}"/>.
/// </summary>
public interface IQuery
{
}

/// <summary>
/// Defines a contract for handling queries in the application layer.
/// Queries represent read operations that do not modify application state.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

### Usage at Call Site

Handlers are injected directly - no `ISender` or mediator intermediary:

```csharp
// Before (MediatR)
internal sealed record GetConfiguredNismsQuery : IRequest<ErrorOr<IEnumerable<NismResponse>>>;
internal sealed class GetConfiguredNismsEndpoint : IGetConfiguredNismsEndpoint
{
    private readonly ISender _sender;

    public GetConfiguredNismsEndpoint(ISender sender)
    {
        _sender = sender;
    }
    
    public async Task<ErrorOr<IEnumerable<NismResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetConfiguredNismsQuery(), cancellationToken).ConfigureAwait(false);
    }
}

// After (custom abstraction)
internal sealed record GetConfiguredNismsQuery : IQuery;
internal sealed class GetConfiguredNismsEndpoint : IGetConfiguredNismsEndpoint
{
    private readonly IQueryHandler<GetConfiguredNismsQuery, ErrorOr<IEnumerable<NismResponse>>> _handler;

    public GetConfiguredNismsEndpoint(IQueryHandler<GetConfiguredNismsQuery, ErrorOr<IEnumerable<NismResponse>>> handler)
    {
        _handler = handler;
    }
   
    public async Task<ErrorOr<IEnumerable<NismResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _handler.HandleAsync(new GetConfiguredNismsQuery(), cancellationToken).ConfigureAwait(false);
    }
}
```

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Licensing | Eliminates commercial dependency and associated cost/compliance risk |
| Simplicity | Removes an indirection layer unused beyond basic dispatch |
| Transparency | Full ownership of dispatch logic; no hidden reflection or dynamic proxy behavior |
| Testability | Handlers are tested directly via their interface, no mock mediator required |
| DI Alignment | Leverages the existing .NET DI container idioms already present in the solution |
| Performance | No runtime reflection, other than the startup registration of application use case handlers |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| Loss of pipeline behavior support | Application validation will be performed manually, as an orchestration step |
| Handler resolution errors surfacing at runtime | Covered by existing integration test suite that exercises the DI container on startup |
| Increased boilerplate per feature | Marginal - one interface injection vs. `ISender`; offset by elimination of `IRequest<T>` marker on every command/query |

## Alternatives Considered

No alternative libraries were evaluated. The sole driver for replacing MediatR was the licensing change, and the solution's usage is narrow enough that a first-party implementation is both sufficient and lower risk than adopting a different third-party mediator library.
