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
| Raw SQL duplication            | All raw SQL is confined to the two bulk repositories, behind the repository and unit of work abstractions |
| Concurrency errors             | WAL and a busy timeout are enabled at startup, and the bulk operations run on dedicated connections |
| Layer integrity                | Dapper is referenced only by the DataAccess layer                 |

## Alternatives Considered

### 1. EF Core `ExecuteSqlRaw` on the context connection
Bulk statements would run on the same connection as the tracked queries of the job.

**Rejected**: It couples the bulk operations to the lifetime of the EF Core context, and parameterized multi-row statements are considerably more verbose without a mapper.

### 2. Third party bulk library (for example EF Core bulk extensions)
A dedicated bulk extension library would abstract the raw SQL.

**Rejected**: Dapper is a small, widely adopted and dependency free mapper, and the amount of raw SQL is small and confined to two repositories. See the ADRs of the scan subsystem for the exact statements.
