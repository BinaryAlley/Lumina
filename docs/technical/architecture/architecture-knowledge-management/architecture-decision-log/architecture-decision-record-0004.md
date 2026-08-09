# Architecture Decision Record (ADR): 0004 - `LibraryScanSnapshots` Table and Database-Side Diff

## Status
**Accepted** (2026-08-08)

## Context
The media library scanning subsystem needs to compute, on every scan, which file system items are new, which are changed and which were deleted since the previous scan. 

This approach has two fundamental problems at the target scale (millions of files):

1. **Memory**: loading the whole previous state into memory as Entity Framework tracked entities costs hundreds of megabytes to gigabytes, which risks out of memory crashes on long running scans.
2. **Accumulation**: `LibraryScanResults` stored one row per changed file per scan, growing unboundedly. The read query joined the whole accumulated table and grouped by path, degrading with every scan.

## Decision
A new table, `LibraryScanSnapshots`, keyed by `(LibraryId, Path)`, holds one row per media library item, representing the current state of every previously scanned media item. The previous state is therefore always available in the database, and the comparison between the files on disk and the previous state is performed with a single database-side diff (an `UPDATE ... FROM` in the staging results), instead of an in-memory join.

The `LibraryScanResults` table is retained for audit purposes only. It records one row per new, modified or deleted item per scan, with its status.

## Consequences

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Memory bound          | The previous state is never loaded wholesale into memory  |
| Deletion handling     | Snapshot items not present in the current scan are deleted and reported through `LibraryMediaItemDeletedDomainEvent` |
| Bounded table growth  | The snapshot holds exactly one row per current media item |
| Incremental scans     | Unchanged items are detected by size and last write time comparison, and are not re-hashed |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| Snapshot staleness on failure | The snapshot swap is a single transaction, only committed when every scan job succeeded |
| Database-side diff portability| The diff uses `UPDATE ... FROM`, supported by SQLite 3.33 and newer |

## Alternatives Considered

### 1. Keep the in-memory dictionary, but make it compact
The previous state could be loaded as a `Dictionary<string, (ulong hash, long size, long ticks)>` with `AsNoTracking()` and compact struct values.

**Rejected**: It still keeps the peak memory proportional to the number of files, which is not safe at the target scale of millions of items.

### 2. History table with per-file tombstone rows
Deleted files would be marked with `LibraryScanFileStatus.Deleted` in `LibraryScanResults` and retained.

**Rejected**: It keeps stale rows around forever, contradicts the requirement of maintaining only a snapshot of the current state, and complicates the read path.
