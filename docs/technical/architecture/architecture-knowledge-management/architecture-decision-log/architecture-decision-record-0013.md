# Architecture Decision Record (ADR): 0013 - Replace Mediator Domain Event Publishing with a Custom Implementation

## Status

**Accepted** (2026-08-13)

## Context

Domain event publishing, the mechanism that fires the `IDomainEvent` objects raised by aggregates to their handlers, depends on the Mediator library (`Mediator.Abstractions` and `Mediator.SourceGenerator`).

1. **Event contract**: `IDomainEvent` implements Mediator's `INotification` marker interface.

2. **Handler registration**: Domain event handlers implements Mediator's `INotificationHandler<T>` and are registered by the source-generated `AddMediator` call.

3. **Publishing sites**: Mediator's `IPublisher` is used from `EventualConsistencyMiddleware` (queued events, published after the transaction commits), from `MediaLibraryScanningService` (scan start events), and from the scan jobs and `ScanFailurePublisher` (progress and failure events).

The remaining publish half carries the same third-party dependency and source-generated registration that ADR-0011 removed from the dispatch half.

## Decision

Replace Mediator's notification publishing with a first-party domain event publisher.

### New Abstractions (in `Lumina.Domain.Common.Events`)

- `IDomainEvent` no longer inherits `INotification`.
- `IDomainEventPublisher` exposes `ValueTask PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)`.
- `IDomainEventHandler<TDomainEvent>` exposes `ValueTask HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken)`. The type parameter is invariant so handlers stay mockable by dynamic proxy libraries.

### Publisher Implementation (`DomainEventPublisher` in Infrastructure)

- Scoped, resolving handlers for the event's runtime type from the DI container.
- Dispatches through a compiled delegate cached per event type in a static `ConcurrentDictionary`, built once via `MakeGenericMethod` over a generic dispatch helper. Handlers run sequentially in registration order, exceptions propagate, and events without handlers are no-ops.
- Registered next to the existing `IDomainEventsQueue`.

### Registration (Application DI)

The existing assembly scan that registers `ICommandHandler<>`, `IQueryHandler<>`, and `IValidator<>` now also registers `IDomainEventHandler<>` as scoped, replacing the source-generated `AddMediator`.

### Consistency Guarantees Preserved

- Commit-before-publish ordering in `EventualConsistencyMiddleware` is unchanged (see ADR-0002).
- Re-entrant publishing (a handler publishing another event) is safe: each publish resolves handlers fresh from the scoped provider.
- Jobs publish from their own scopes, so handlers receive a fresh unit of work and `DbContext`.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Dependency reduction | Removes `Mediator.Abstractions` and `Mediator.SourceGenerator` from the entire solution |
| Transparency | Dispatch logic is fully owned and inspectable; no generated code |
| Consistency | Domain event handlers join the same explicit registration scan as the CQRS handlers |
| Single event contract | `IDomainEvent` remains the sole contract until a real cross-bounded-context consumer exists |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| Handler resolution failures surface at runtime | Existing integration test suite exercises the DI container on startup |
| Reflection-based dispatch per event type | Compiled delegate cache makes dispatch cheap after the first publish of each type |
| Dynamic proxy cannot proxy generic interfaces | `IDomainEventHandler<TDomainEvent>` is deliberately invariant so handlers remain mockable in unit tests |

## Alternatives Considered

### 1. Domain versus Integration Event Split

Introduce `IIntegrationEvent` now, with a durable outbox, for inter-bounded-context propagation.

**Rejected**: there are no cross-bounded-context consumers today - every event lives in the `LibraryManagementBoundedContext` and is handled in-process. The outbox machinery (durability, retries, idempotency) is speculative until the first real consumer appears, and forcing every existing event into one of two buckets would produce classification with no basis. The seam is documented on `IDomainEvent` itself so the split stays cheap to introduce later.

### 2. Keeping Mediator for Publishing

Keep the Mediator library for the domain event publishing half.

**Rejected**: for the same reason ADR-0011 rejected it for command and query dispatching - an external dependency, with source-generated registration, for functionality that is fully implementable in-house.
