# Architecture Decision Record (ADR): 0009 - Failure Semantics of the Media Library Scan Jobs

## Status
**Accepted** (2026-08-08)

## Context
Media library scan jobs are launched in a fire-and-forget manner from the background job processor (`MediaLibraryScanJobProcessorJob`), because only the root jobs are placed on the in-memory queue, and they trigger their children recursively (see [ADR-0001](architecture-decision-record-0001.md) and [ADR-0008](architecture-decision-record-0008.md)). The launch mechanism uses `FireAndForgetSafeAsync`, which observes the launched task but does not crash the queue consumer.

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

## Amendment: Awaiting the Scan in the Serialized Scan Processor (2026-10-31)

The rejection of alternative 1 rested on an assumption that no longer holds: that the queue consumer must never block. That assumption produced the defect that forced this amendment. Launching every root job in a fire-and-forget manner meant that scanning several libraries at once ran one complete job chain per library simultaneously, each of them fanning out to as many parallel file reads as the machine has processors, all on the thread pool shared with the web server, so the API could become unresponsive for the duration of the scans. Media library scans are now deliberately serialized, one at a time. The next queued scan must not start before the current one has stopped, which is only guaranteed if the processor waits for it.

Alternative 1 is therefore the decision, and the fire-and-forget launch is removed. The failure semantics decided here are unchanged: every job still catches its own failures and publishes the `LibraryScanFailedDomainEvent`, `OperationCanceledException` still marks the job as `Canceled`, and the handlers still release the scan processing resources.

Because the processor must keep draining the queue after a failed scan, a scan that fails must never affect the scans queued behind it. This matters the most for the action that starts several scans at once: a single library that cannot be scanned, for example because its type has no scanner yet, must not stop the libraries queued behind it. A failure that happens while a scan is being prepared now publishes the same failure event for that scan only, instead of being swallowed, and the processor moves on to the next queued scan.
