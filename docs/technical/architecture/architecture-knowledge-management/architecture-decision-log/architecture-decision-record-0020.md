# Architecture Decision Record (ADR): 0020 - Use GUID Version 7 for Persisted Identifiers

## Status
**Accepted** (2026-11-16)

## Context

Persisted entity identifiers were generated with `Guid.NewGuid()`, which produces a version 4 GUID with 122 random bits and no ordering. SQLite stores the identifiers as upper case, hyphenated TEXT values, and the primary key is the index key, so random identifiers scatter inserts across the index instead of appending to it.

Identifiers are minted in several places:

- Domain value object factories, mappings, and aggregate methods that create domain event identifiers.
- Application command handlers that build repository entities.
- Data access repositories and the data seed service.
- Infrastructure media library scan jobs, provider configuration storage, scheduler execution, and theme synchronization.
- Entity Framework Core, for keys configured with `ValueGeneratedOnAdd`, including real keys such as `PermissionEntity.Id` and `RoleEntity.Id`, and shadow keys of owned collections such as `AlbumReleaseTypes.Id`.
- Two bulk inserts that generate their identifiers in SQLite to keep a single statement.

A search for `Guid.NewGuid` cannot see the Entity Framework Core and SQL paths. Those paths would otherwise keep producing version 4 identifiers while the rest of the solution moves to version 7.

## Decision

Persisted entity and domain event identifiers are created with `Guid.CreateVersion7()`, which embeds a Unix timestamp in milliseconds in the most significant bits and makes the identifiers time ordered.

The decision applies to Domain, Application, DataAccess, and Infrastructure:

- Every identifier factory and every code path that mints an entity or domain event identifier in application code calls `Guid.CreateVersion7()` directly.
- Every Entity Framework Core property of type `Guid` with `ValueGenerated.OnAdd` is assigned the `GuidV7ValueGenerator` in `LuminaDbContext.OnModelCreating`. The loop covers real keys and owned collection shadow keys, so the data seed service can keep leaving seeded permission and role identifiers unset.
- Existing rows keep their version 4 identifiers. This is a forward-only change, and no primary key or foreign key is rewritten.

The following identifiers intentionally remain outside the version 7 strategy:

- **Opaque playback delivery handles** in `PlaybackDeliveryHandleService` stay version 4. The handle is deliberately unguessable and must not reveal when it was created, and version 7 embeds the creation time and leaves fewer random bits.
- **Plugin identifiers** in `PluginDetectionSyncJob.DerivePluginId` are derived with SHA-256 from the plugin assembly file name, so a plugin that fails to load keeps the same row across restarts. A derived, stable value is required, not a time ordered one.
- **The SYSTEM audit user identifier** in `UpdateAuditableEntitiesInterceptor` is the fixed sentinel `11111111-1111-1111-1111-111111111111`.
- **The bulk insert paths** in `PlaylistRepository.InsertScopeEntriesAsync` and `LibraryScanSnapshotRepository.ApplySnapshotSwapAsync` generate random version 4 GUIDs in SQLite. Generating the identifiers in C# would materialize the rows and defeat the single statement insert. SQLite cannot produce a version 7 GUID inside those statements without a custom function.
- **Temporary file names, cache reservation and deletion names, theme work directories, and UI script identifiers** are not persisted identifiers, so they stay version 4.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Index locality | Time ordered keys append to the primary key index instead of scattering across it |
| Diagnostics | The creation time is recoverable from a persisted identifier |
| Coverage | Entity Framework Core keys and owned collection shadow keys follow the same strategy as the code that assigns identifiers explicitly |
| Consistency | The solution has one strategy for persisted identifiers, with the exceptions recorded above |
| Compatibility | The identifier type and storage format are unchanged, so no schema migration is required |

### Risks and Tradeoffs

| Risk | Mitigation Strategy |
|---|---|
| The creation time is embedded in the identifier | Do not use version 7 identifiers where the creation time must remain secret |
| Version 7 has fewer random bits than version 4 | Keep version 4 for opaque security handles, and never treat an identifier as a secret |
| Existing rows keep version 4 identifiers | Do not use identifier order as a chronology across the migration boundary; use `CreatedOnUtc` |
| The two SQL bulk insert paths keep version 4 identifiers | The exception is documented in this ADR and in both repositories, so it is not changed accidentally |
| Version 7 ordering depends on the system clock | Clock skew can break ordering, but not uniqueness; do not depend on strict ordering across machines |
| Entity Framework Core could seed a new key with version 4 again | `LuminaDbContext` applies the generator to every `Guid` property configured with `ValueGenerated.OnAdd`, including owned shadow keys |

## Alternatives Considered

### 1. Keep version 4 identifiers

Keep `Guid.NewGuid()` everywhere and accept random primary key inserts.

**Rejected**: random keys scatter inserts across the SQLite primary key index, which is the write path of every media library scan and every catalogue edit.

### 2. Generate sequential or COMB GUIDs in application code

Implement a custom monotonic GUID generator that places a timestamp or counter in the version 4 layout.

**Rejected**: it requires custom generation and ordering logic that must be maintained and tested, while .NET provides a standard time ordered version 7 GUID.

### 3. Generate version 7 identifiers in SQL for the bulk inserts

Construct the version 7 timestamp and random fields inside the bulk SQL statements.

**Rejected**: SQLite has no built-in version 7 GUID function, and adding a custom function or extension for these two paths would add deployment and portability constraints. The single statement inserts keep their version 4 identifiers and the exception is documented.

### 4. Rewrite existing identifiers to version 7

Migrate every stored primary key and foreign key to a version 7 value.

**Rejected**: rewriting primary keys means rewriting every foreign key that references them, with a high risk of corruption and downtime, for no benefit to existing rows.
