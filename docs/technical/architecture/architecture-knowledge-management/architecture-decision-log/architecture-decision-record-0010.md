# Architecture Decision Record (ADR): 0010 - Failure Semantics of the Best-Effort Metadata Enrichment Job

## Status
**Accepted** (2026-08-09)

## Context
ADR-0009 established that every media library scan job wraps its execution in a catch-all that marks the job as `Failed` and publishes the `LibraryScanFailedDomainEvent`. That decision applies to the mandatory scan jobs: file system discovery, scan diff, content hashing and scan results save. These jobs are transactional in nature, and a failure in any of them means the file state of the library was not committed.

The metadata enrichment phase runs after the scan results save job, once the file state of the library is committed and the books are materialized. The enrichment phase is best-effort: a book whose metadata could not be retrieved is still browsable with its shell metadata (path and filename-derived title), and a crash mid-enrichment must not roll back the already committed scan results, nor hide the successfully enriched books. This was an explicit product decision: partial enrichment survives a crash, and the scan is not considered failed because a remote metadata provider was unreachable or a provider crashed.

## Decision
The metadata enrichment job (`MediaLibraryScanMetadataEnrichmentJob`) is a best-effort job with failure semantics that differ from the mandatory jobs of ADR-0009:

- Per-book failures are isolated: a metadata provider that throws, returns unusable metadata, or fails to be applied marks the book as `Failed` (`MetadataStatus.Failed`) and moves to the next book. The other providers are still tried for the same book, and the other books are still processed.
- Per-provider failures are isolated: a crashing metadata provider does not prevent the other providers from being tried, in their configured order.
- The scan is considered completed (`LibraryScanFinishedDomainEvent`) after the enrichment phase, even when some books failed to be enriched. The failure is recorded on the books, not on the scan.
- Only a host-side catastrophic failure of the enrichment job itself (e.g., a storage error) fails the scan, through the existing catch-all of ADR-0009. In that case the file state is already committed, and a re-scan retries the enrichment.

The `MediaLibraryScanResultsSaveJob` no longer publishes `LibraryScanFinishedDomainEvent`. The event is published by the last job of the directed acyclic job graph: the save job when the library does not permit downloading data from the web (no enrichment phase), or the metadata enrichment job otherwise.

## Consequences

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Partial enrichment    | Books successfully enriched before a crash stay browsable and enriched |
| Provider isolation    | A crashing provider does not prevent other providers from being tried |
| Scan completion       | The scan reaches a completed state regardless of enrichment outcome |
| Retryability          | Books with `MetadataStatus.Failed` or `Pending` are re-enriched on the next scan |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| Host failures still fail      | Only host-side storage errors fail the scan; re-scan retries enrichment |
| No per-provider audit trail   | Books record the failed status; per-provider error detail is a future refinement |

## Alternatives Considered

### 1. Fail the scan on any enrichment failure
Keep ADR-0009 semantics for the enrichment job.

**Rejected**: The file state is already committed, and partial enrichment is better than failing a scan that saved the library. Failing the scan would also roll back nothing, while misleading the user about the scan outcome.

### 2. Run enrichment outside the scan lifetime
Enrich the books asynchronously after the scan, decoupled from the scan progress and cancellation.

**Rejected**: This contradicts the decision that the metadata enrichment phase is part of the scan lifetime, covered by the scan progress reporting and cancellation machinery, so that the scan progress reaches completion only when the metadata is downloaded.
