# Architecture Decision Record (ADR): 0006 - Dapper and SQLite WAL for Bulk Data Access in Scan Jobs

## Status
**Accepted** (2026-08-08)

## Context
The media library scan jobs perform bulk data operations that the Entity Framework Core change tracker is not suited for:

1. multi-row inserts of the discovered file system items into the staging results,
2. multi-row upserts of the scan snapshot,
3. indexed key-set paginated reads of the items that need hashing,
4. raw `DELETE` statements for pruning and clearing.

Bulk inserts through the EF Core change tracker are slow for hundreds of thousands of rows, because every entity is individually tracked and saved. At the target scale (millions of files), the scan pipeline needs a data access path that is not only fast, but also bounded in memory.

## Decision
Add the Dapper library to the DataAccess layer, and perform the bulk data operations of the scan jobs with parameterized raw SQL on a dedicated `Microsoft.Data.Sqlite` connection, derived from the connection string of the injected `LuminaDbContext`.

These operations never touch the Entity Framework change tracker. Reads that are not bulk (for example loading the media library entity) keep using EF Core, and bulk reads use unbuffered, key-set paginated queries to keep the peak memory bounded.

SQLite Write-Ahead Logging (`PRAGMA journal_mode=WAL`) and a busy timeout are enabled at API startup, so that the Entity Framework connection and the dedicated bulk connections of the scan jobs can access the database concurrently, without database locking errors.

## Consequences

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Performance           | Multi-row inserts and upserts are orders of magnitude faster than per-row EF Core inserts |
| Memory bound          | Unbuffered, key-set paginated reads keep peak memory proportional to the batch size |
| Tracking safety       | The bulk operations bypass the EF Core change tracker entirely |
| Concurrency           | WAL allows one writer and multiple concurrent readers      |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| Raw SQL duplication            | All raw SQL is confined to the bulk repositories (the scan jobs and the documented playlist append), behind the repository and unit of work abstractions |
| Concurrency errors             | WAL and a busy timeout are enabled at startup, and the bulk operations run on dedicated connections |
| Layer integrity                | Dapper is referenced only by the DataAccess layer                 |

## Alternatives Considered

### 1. EF Core `ExecuteSqlRaw` on the context connection
Bulk statements would run on the same connection as the tracked queries of the job.

**Rejected**: It couples the bulk operations to the lifetime of the EF Core context, and parameterized multi-row statements are considerably more verbose without a mapper.

### 2. Third party bulk library (for example EF Core bulk extensions)
A dedicated bulk extension library would abstract the raw SQL.

**Rejected**: Dapper is a small, widely adopted and dependency free mapper, and the amount of raw SQL is small and confined to the bulk repositories. See the ADRs of the scan subsystem for the exact statements, and the amendment below for the playlist append.

## Amendment: Playlist Whole-Library Appends (2026-10-20)

The playlist feature needs to append a whole library, which can contain hundreds of thousands of tracks, to a playlist in one request. The Entity Framework change tracker cannot materialize that graph, so `PlaylistRepository.AddEntriesAsync` performs the append with parameterized multi-row `INSERT` statements in bounded batches.

Unlike the scan jobs, this bulk operation deliberately uses the connection and the transaction of the request's unit of work, not a dedicated connection. The request middleware already holds the SQLite write transaction, and a dedicated connection would wait on that lock while the request waits on the append. Sharing the request transaction keeps the append atomic with the ownership check and the activity update, and the busy timeout plus the unique `(PlaylistId, Position)` index turn a concurrent append into a mapped conflict instead of a duplicate position.

The exception is confined to the append and the rare sparse-key rebalance in `PlaylistRepository` and is documented in the class remarks. Moves and removals use Entity Framework `ExecuteUpdateAsync` and `ExecuteDeleteAsync` on their normal path, so they keep the model, the transaction and the error mapping. The rebalance uses raw SQL only when repeated midpoint moves exhaust the numeric gap between two neighbors; it respaces the order keys through a temporary table inside the request transaction.

## Amendment: Playlist Scope Ordering and Rollback (2026-10-21)

A whole library, artist or album scope is now appended with one `INSERT ... SELECT` statement in `PlaylistRepository.AddScopeEntriesAsync`. The statement joins the catalogue tables, orders the tracks with `ROW_NUMBER()` in SQLite and computes the sparse positions as part of the insert, so the scope is never materialized, sorted in memory or sent to the data access layer as a list of Ids. Explicit track lists still use the batched `AddEntriesAsync` path, because that list is already in memory.

An append that fails or is cancelled rolls the request transaction back explicitly. The context records that rollback, and the eventual consistency middleware skips the commit and logs it when the response still reports success, instead of committing a partially written operation.

## Amendment: Request Transaction Ownership and Aborted Requests (2026-10-21)

The eventual consistency middleware now begins, commits and rolls back the request transaction through `IUnitOfWork`, so the transaction boundary follows the unit of work instead of the `DbContext` being used directly. The commit-before-publish sequence of ADR-0002 is unchanged.

An aborted request is no longer left at its default success status: the exception handling middleware reports it as `499 Client Closed Request` and rethrows. The eventual consistency middleware therefore decides from the response status alone. It commits a 2xx response and rolls back every other response, including an abort, and it deliberately does not consult the request abort token, because that token can already be cancelled after a successful operation whose response was already sent.

This replaces the in-context rollback marker and the repository's rollback of the ambient request transaction described in the previous amendment. A repository now only commits or rolls back a transaction it opened itself, which happens when it runs without an ambient request transaction. A partially written append is therefore discarded by the middleware's rollback, not by a signal from the repository.
