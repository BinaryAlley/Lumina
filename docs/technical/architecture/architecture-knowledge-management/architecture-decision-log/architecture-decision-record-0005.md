# Architecture Decision Record (ADR): 0005 - Permanent `LibraryScanStagingResults` Table, Cleaned per Scan

## Status
**Accepted** (2026-08-08)

## Context
The media library scan pipeline stages the file system items discovered during a scan before comparing them against the `LibraryScanSnapshots` table (ADR-0004). A staging store is needed that:

1. receives the discovered file system items in bounded batches,
2. is read by the diff, hash and metadata enrichment jobs,
3. is written by the discovery job and read by later jobs, which run in separate dependency injection scopes (see ADR-0001), hence on separate database connections.

A real SQLite `TEMP` table was considered, but SQLite `TEMP` tables are scoped to a single database connection. Because each media library scan job creates its own `AsyncServiceScope`, and therefore its own `LuminaDbContext` and connection (ADR-0001), a `TEMP` table created by the discovery job would be invisible to the diff job, which runs on a different connection. Making a `TEMP` table work would require pinning a single dedicated connection for the whole staging phase of a scan, a larger architectural carve-out.

## Decision
Use a permanent table, `LibraryScanStagingResults`, keyed by `(LibraryScanId, Path)`. Its rows are deleted at the end of the scan (as part of the snapshot swap transaction of the save job) and on scan failure or cancellation. Leftover rows of a crashed scan are inert, because every scan has a unique id, and can be purged by the audit cleanup job.

## Consequences

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Connection safety     | The table survives the per-job connection churn           |
| Debuggability         | The staging state can be inspected mid-scan                |
| Cheap cleanup         | Clearing is a single indexed `DELETE` per scan            |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| Leftover rows on process crash | They are inert (scan id is unique) and cleaned by the audit cleanup job |
| Extra table                   | Bounded at one row per discovered item per scan, cleared each scan |

## Alternatives Considered

### 1. SQLite `TEMP` table with a single pinned connection
The whole staging phase would hold one dedicated connection open, and all jobs would share it.

**Rejected**: It conflicts with the per-job scope model of ADR-0001, holds a database connection open for the entire scan duration, and makes the pipeline harder to compose and test.

### 2. In-memory staging (list of discovered items passed between jobs)
The discovery job would pass the full list of discovered items to the diff job.

**Rejected**: It brings back the unbounded memory problem that ADR-0004 eliminates. The staging table is what allows bounded, batched writes.
