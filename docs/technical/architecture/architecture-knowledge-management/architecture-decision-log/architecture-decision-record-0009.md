# Architecture Decision Record (ADR): 0009 - Failure Semantics of the Media Library Scan Jobs

## Status
**Accepted** (2026-08-08)

## Context
Media library scan jobs are launched in a fire-and-forget manner from the background job processor (`MediaLibraryScanJobProcessorJob`), because only the root jobs are placed on the in-memory queue, and they trigger their children recursively (see ADR-0001 and ADR-0008). The launch mechanism uses `FireAndForgetSafeAsync`, which observes the launched task but does not crash the queue consumer.

The scan jobs must report their failures through the existing eventual consistency mechanism, the `LibraryScanFailedDomainEvent`, whose handler marks the scan as failed, releases the scan processing resources and notifies the clients. The previous job implementations only published the failure event on a few specific error paths, so any other exception was silently swallowed, leaving the scan stuck in the `Running` state, leaking its cancellation source and progress entry.

## Decision
Keep the fire-and-forget launch mechanism. Every scan job wraps its execution in a catch-all that marks the job as `Failed` and publishes the `LibraryScanFailedDomainEvent`, carrying the exception message for diagnostics. `OperationCanceledException` keeps marking the job as `Canceled`.

The `LibraryScanFailedDomainEventHandler` and the `LibraryScanCancelledDomainEventHandler` now always release the scan processing resources, regardless of whether the scan was already marked as failed or cancelled by a concurrent job: the cancellation token source is removed from the cancellation tracker, the progress entry is removed from the progress tracker, and the staging results of the scan are cleared.

## Consequences

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Reliable failure      | Every job failure is reported through the failure domain event |
| Resource cleanup      | Cancellation sources, progress entries and staging rows are released on failure and cancellation |
| Eventual consistency  | The handlers perform the cleanup, keeping the job code focused on scanning |
| Configurable actions  | The failure event can drive further handlers (notifications, etc.) in the future |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| Best-effort reporting         | Failure reporting is wrapped defensively, so it never crashes the job processing |
| Duplicate cleanup             | The cleanup operations are idempotent and tolerant of concurrent executions |

## Alternatives Considered

### 1. Await the whole job chain in the background processor
Change the processor to await the root job completion and observe its exceptions there.

**Rejected**: Awaiting long running job chains in the queue consumer would introduce blocking behavior in the processor.

### 2. Rethrow the exceptions to be swallowed by the fire-and-forget helper
Keep the current behavior of swallowing unexpected exceptions.

**Rejected**: It leaves scans stuck in the `Running` state and leaks resources, which the whole point of this decision is to fix.
