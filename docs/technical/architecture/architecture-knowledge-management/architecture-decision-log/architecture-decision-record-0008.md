# Architecture Decision Record (ADR): 0008 - Plugin Hook Points and the Media Library Scan Job Graph

## Status
**Superseded by [ADR-0015](architecture-decision-record-0015.md)** (2026-10-09)

## Context
The media library scanning subsystem is meant to be modular: generic jobs, like the scan diff, hashing and results save jobs, are shared by all media library types, while media-type specific jobs, like the GoodReads metadata scrap job for books or a TMDB cover art job for movies, are decoupled and composed into a directed acyclic graph.

The job graph must remain fully capable of multi-parent relationships. Even though the default composition for books is linear (file system discovery, scan diff, hashing, optional metadata enrichment, scan results save), a plugin might inject jobs with several parents and children, and the base job model (`MediaLibraryScanJob` with `Parents`, `Children`, `AddChild` and `AddParent`) supports this.

Plugins should be injectable "at any sensible point" of the graph, not literally at any point. For example, a job that runs before the file system discovery has little meaning, because nothing is known about the library contents yet.

## Decision
Define named hook points for the scan job graph in `ScanJobHooks`: `AfterFileSystemDiscovery`, `AfterScanDiff`, `BeforeMetadataEnrichment`, `AfterMetadataEnrichment` and `BeforeScanResultsSave`.

Define the `IScanJobRegistry` interface, which maps a hook name to the plugin jobs registered at that hook. The media type scanners (for example `BookLibraryTypeScanner`) splice the plugin jobs returned by the registry into the graph at the corresponding hook points, preserving the parent-child relationships. The composition root of the plugin system provides the registry with the plugin job types; the default registry registers no plugin jobs.

The payload passing between jobs keeps the generic `ExecuteAsync<TInput>` contract. The core scan jobs are database-centric and ignore the payload, because they exchange the scan state through the staging and snapshot tables ([ADR-0004](architecture-decision-record-0004.md), [ADR-0005](architecture-decision-record-0005.md)). Payload passing remains available for plugin jobs that need to exchange data in memory, and the runtime type switching that was used in the previous `HashComparerJob` is eliminated.

## Amendments

- **2026-10-09**: Superseded by [ADR-0015](architecture-decision-record-0015.md). The `ScanJobHooks` hook points and the `IScanJobRegistry` / `ScanJobRegistry` were removed, and plugins now contribute capability providers (`IMetadataProvider`, `IArtworkProvider`, `IBookReader`) instead of scan jobs. This ADR is marked superseded rather than amended because its decision is replaced in full by [ADR-0015](architecture-decision-record-0015.md); [ADR-0001](architecture-decision-record-0001.md), [ADR-0003](architecture-decision-record-0003.md) and [ADR-0010](architecture-decision-record-0010.md) stay accepted because only part of their decisions changed.

## Consequences

> The rows struck through reflect the superseded hook point and scan job registry model. They no longer hold, see [ADR-0015](architecture-decision-record-0015.md).

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Modularity            | Media-type specific jobs are composed per scanner          |
| ~~Extensibility~~     | ~~Plugins inject jobs at documented hook points~~ (removed, see [ADR-0015](architecture-decision-record-0015.md)) |
| Multi-parent support  | The base job model keeps full parent-child capabilities    |
| ~~Fail fast composition~~ | ~~Wrong payload wiring produces loud errors instead of silent no-ops~~ (removed, see [ADR-0015](architecture-decision-record-0015.md)) |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| ~~Plugin job conflicts~~      | ~~Hook points are named and documented, the composition is validated when the graph is built~~ (removed, see [ADR-0015](architecture-decision-record-0015.md)) |
| Payload contract misuse        | Jobs that expect a payload fail loudly when the edge type does not match |
| ~~Scope creep~~               | ~~Only the sensible hook points are defined, arbitrary injection is not supported~~ (removed, see [ADR-0015](architecture-decision-record-0015.md)) |

## Alternatives Considered

### 1. Arbitrary injection at any junction, including before discovery
Allow plugins to attach anywhere in the graph without restriction.

**Rejected**: Several junctions are meaningless before the library contents are known. The named hook points cover the useful extension surface, and keep the composition understandable.

### 2. A full dataflow engine with typed edges between every pair of jobs
Rework the jobs into a generic pipeline engine where edges carry strongly typed channels.

**Rejected**: The database-centric jobs exchange state through the database, so a full in-memory typed dataflow engine would add complexity without benefit. The generic payload contract is retained for the plugin jobs that need it.
