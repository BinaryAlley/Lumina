# Architecture Decision Record (ADR): 0007 - Opt-in Directory Fast Skip Using Directory Scan Fingerprints

## Status
**Accepted** (2026-08-08)

## Context
The default media library scan walks every directory and enumerates every file of the library on every scan. Even when unchanged files are not re-hashed (see `MediaLibraryScanDiffJob`), the walk itself is proportional to the total number of files. For libraries with tens of millions of files, the walk can dominate the scan duration.

A fast path that skips unchanged directories was considered. It is fundamentally based on directory metadata, which has two important limitations:

1. editing the contents of an existing file does not change the last write time of its containing directory,
2. a change deep inside a subtree does not propagate to the last write time of the ancestor directories.

Because of these limitations, the fast path can miss changes, and it must never be the default behavior of a scan.

## Decision
Add a `DirectoryScanFingerprints` table, keyed by `(LibraryId, Path)`, storing the last write time of each directory of a library. Add a `ShouldSkipUnchangedDirectoriesDuringScan` option to the `Library` aggregate, which is **off by default**.

When the option is enabled, the file system discovery job compares the last write time of every visited directory against the stored fingerprint. When they match, the entire subtree of that directory is skipped, and when they differ, or no fingerprint exists, the directory is walked and its fingerprint is updated.

The option is presented to the user with a descriptive text that explains its benefits and its risks, and it is documented here as well: the fast path relies on directory metadata, which does not change when existing files are edited, and does not propagate upward for changes that occur deep inside a subtree. It is only recommended for users that never modify existing files in place.

## Consequences

### Positive Outcomes
| Aspect                | Benefit                                                   |
|-----------------------|-----------------------------------------------------------|
| Scan duration         | Unchanged subtrees are skipped with a single metadata stat per directory |
| User choice           | The fast path is opt-in, the deterministic full walk remains the default |
| Existing behavior     | Default scans keep the exact same semantics as before      |

### Risks and Tradeoffs
| Risk                          | Mitigation Strategy                                               |
|-------------------------------|-------------------------------------------------------------------|
| Missed in-place edits          | Documented to the user, the option is off by default              |
| Missed deep changes            | Documented in this ADR and in the option description, the option is off by default |
| Coarse timestamp granularity   | Some file systems store directory timestamps with a coarse resolution, documented as a limitation |

## Alternatives Considered

### 1. Full walk with per-file comparison only
No directory skipping at all; the walk enumerates every file, but only changed files are processed.

**Rejected as the default**: This is exactly the default behavior, but it does not reduce the cost of the walk itself.

### 2. Subtree-aggregate fingerprints
Store a fingerprint that aggregates the entire subtree of a directory, so any change anywhere bubbles up to the root.

**Rejected**: Computing a subtree aggregate requires walking the subtree, which defeats the purpose of the fast path.
