# Architecture Decision Record (ADR): 0002 - Transaction commit before event publishing in Eventual Consistency Middleware

## Status
**Accepted** (2025-02-18)

## Context
The system requires reliable coordination between database transactions and domain event processing in scenarios involving:

1. **Background service isolation**: Long-running operations triggered by domain events execute in singleton-hosted services with separate dependency injection scopes.
2. **Transaction visibility requirements**: Subsequent operations in child scopes must see committed database state.
3. **Eventual consistency needs**: Event handlers may spawn follow-up operations requiring atomic transaction boundaries.

The previous implementation published domain events *before* transaction commit, causing:

| Problem                   | Impact                                                                    |
|---------------------------|---------------------------------------------------------------------------|
| Phantom reads             | Background services saw pre-commit state via separate DbContext instances |
| Stale data actions        | Event handlers executed against uncommitted changes                       |
| Scope lifetime mismatch   | Singleton services couldn't access request-scoped transaction context     |

## Decision
Enforce strict commit-before-publish sequence in `EventualConsistencyMiddleware`:

```csharp
context.Response.OnCompleted(async () =>
{
    if (IsSuccessfulResponse(context))
    {
        await transaction.CommitAsync(); // commit FIRST
        await PublishEvents(publisher, domainEventQueue); // THEN publish
    }
    await transaction.DisposeAsync();
});
```


## Consequences

### Positive Outcomes

| Aspect            | Benefit                                                           |
|-------------------|-------------------------------------------------------------------|
| Data visibility   | Guarantees event handlers see committed state                     |
| Atomicity         | Prevents partial updates from being visible                       |
| Error recovery    | Failed event publishing doesn't rollback committed transactions   |
| Scope alignment   | Matches DbContext lifetime with transaction boundaries            |

### Risks and Tradeoffs

| Risk                          | Mitigation Strategy                                                   |
|-------------------------------|-----------------------------------------------------------------------|
| Event loss after commit       | Implement dead-letter queue with compensation handlers (to be done)   |
| Non-atomic event publishing   | Use outbox pattern for critical events (to be done)                   |
| Transaction timeout           | Configure SQL Server `SET LOCK_TIMEOUT` appropriately                 |

## Alternatives Considered

### 1. Transactional Outbox Pattern
```csharp
// store events in outbox table within transaction
dbContext.OutboxMessages.AddRange(events);
await dbContext.SaveChangesAsync();

// separate worker publishes from outbox
```

**Rejected** for non-critical events due to:
- Added infrastructure complexity (outbox table + worker)
- Increased latency for non-critical notifications
- Overkill for non-idempotent operations

### 2. Two-Phase Commit (2PC)
```csharp
using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
{
    // distributed transaction coordination
}
```
**Rejected** due to:
- Unsupported in polyglot persistence environment
- Performance overhead from MSDTC coordination
- Conflict with cloud-native architecture principles
