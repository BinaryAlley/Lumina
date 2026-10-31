# Architecture Decision Record (ADR): 0019 - SSD Aware Read Parallelism During Media Library Scans

## Status
**Accepted** (2026-10-30)

## Context

A media library scan performs two kinds of file system reads. Discovery reads directory entries and file metadata, and hashing samples file contents. Their throughput depends on the storage medium. A solid state drive serves concurrent reads with little penalty, while a mechanical hard drive pays a seek for every position change, so concurrent reads can reduce its throughput.

Lumina cannot determine the storage medium reliably. Drive type queries are platform specific, and they are unreliable for network, virtual, and removable volumes. The scan therefore needs an explicit input instead of an automatic guess.

Not every scan stage performs bulk local reads. Audio metadata extraction parses tag blocks, and metadata and artwork enrichment call remote providers. Those stages have their own constraints, which are part of this decision.

## Decision

Add an `IsContentOnSsd` boolean to the `Library` aggregate, **off by default**. It is stored in the `Libraries` table with a default value of `false` and exposed through the library form and API contracts.

The setting controls read parallelism in the two stages that perform bulk local reads:

- **File system discovery**: the book and music discovery jobs read the sizes and last write times of the files of each directory in parallel when the option is on, and sequentially when it is off. Discovered files and directory fingerprints are still merged and written to the repositories sequentially, because the repositories are not thread-safe.
- **File hashing**: `MediaLibraryScanHashJob` reads `IsContentOnSsd` without loading the content location or path template graph, and passes it to `IFileHashService.HashFilesAsync`. The service uses `MaxDegreeOfParallelism = Environment.ProcessorCount` when the option is on, and `MaxDegreeOfParallelism = 1` when it is off.

The following stages keep their current sequencing:

- **Audio metadata extraction**: extraction and persistence are interleaved on one `IUnitOfWork` and `DbContext`, which is not thread-safe. Parallelizing it requires a bounded producer and consumer split, with extraction on worker threads and all repository writes on one thread.
- **Metadata and artwork enrichment**: these stages are network bound, and the providers already have process-wide request throttles. Item level parallelism would mostly queue on those throttles, and it is a provider concurrency concern rather than a storage medium concern.

The setting does not change the stored catalogue or the result of a scan. It only changes how discovery and hashing read the files, so a change takes effect on the next scan and never requires a rehash of unchanged files.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Explicit choice | The scan never guesses the storage medium from platform specific or unreliable drive queries |
| Off by default | Existing libraries keep sequential reads, and mechanical drives are never slowed down by an upgrade |
| Bounded memory | Parallel reads are merged before batching, so staging results and directory fingerprints keep their existing batch size and memory profile |
| Cheap hashing lookup | The hashing job reads only the scalar setting, without tracking the library or loading its navigation properties |

### Risks and Tradeoffs

| Risk | Mitigation Strategy |
|---|---|
| A user enables the option on a mechanical drive | The option label and description state that it is meant for solid state drives, and the manual repeats the warning |
| The option can be forgotten after moving a library to another drive | It is editable on the existing library, and it takes effect on the next scan |
| The content locations of one library can be mixed across solid state and mechanical drives | The option describes the library as a whole. Keep it off when any content location is on a mechanical drive, or split the locations into separate libraries when the difference matters |
| Parallel metadata reads on a directory with many files allocate the metadata array for that directory | Only one directory is processed at a time, and the array is merged into the existing bounded staging batches as soon as it is ready |
| Metadata extraction and enrichment do not benefit from the setting | They are documented as separate stages with their own constraints, and the option description names only file discovery and content hashing |
| Discovery reads and hashing run in parallel, but enrichment still waits on provider throttles | The setting is scoped to local read throughput and does not claim to speed up network bound stages |

## Alternatives Considered

### 1. Always read in parallel

Keep the previous `Environment.ProcessorCount` behavior for every scan and do not expose a setting.

**Rejected**: mechanical drives can become slower with concurrent reads.

### 2. Detect the storage medium automatically

Query the operating system for the drive type of each content location and choose the parallelism accordingly.

**Rejected**: the detection is platform specific, unreliable for network and virtual volumes, and would hide the choice from the user.

### 3. Parallelize metadata extraction and enrichment under the same setting

Use the same flag to parallelize audio metadata extraction, metadata enrichment, and artwork resolution.

**Rejected for now**: extraction needs a bounded producer and consumer refactor around the non-thread-safe `DbContext`, and enrichment is network bound behind process-wide provider throttles. Both are separate concerns from the read behavior of the storage medium.
