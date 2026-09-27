# Architecture Decision Record (ADR): 0014 - Closed Generic Persistence Toolkit for Aggregate Graph Persistence

## Status

**Accepted** (2026-09-26)

## Context

Repositories in the DataAccess layer persist aggregate graphs: they load a tracked root entity, apply the edited scalar values, reconcile child collections, and add or update shared reference rows. This work was implemented independently in each repository, which produced duplication and drift.

1. **Shared reference handling**: a shared reference row (a tag or a genre) is keyed by its name and is shared across the whole database. When a graph is inserted or updated, the stored rows whose names already exist have to be reused instead of re-inserted, so that the shared tables are not duplicated, and a name shared by several parents of one request must be tracked only once. This logic existed inline in `BookRepository`, and as a private `ReconcileTagsAndGenresAsync` method in `BookRepository`, `AlbumRepository`, and `TrackRepository`.

2. **Child collection reconciliation**: the same identity-preserving reconcile logic was repeated for every child collection of every aggregate (contributors, ratings, ISBNs, tags, genres, moods, ISRCs, content locations, permissions).

3. **Scalar copy on edit**: `EditableValuesCopier` applies editable scalar values while leaving the audit columns to the auditing interceptor. Seven auditable repositories (`Role`, `LibraryScan`, `ScheduledJob`, `ScheduledJobExecution`, `Theme`, `User`, `UserSettings`) still copied scalars with `CurrentValues.SetValues`, which writes every scalar property, including the primary key and the audit columns. Writing the primary key is destructive whenever the tracked row was located by a key other than its primary key: `UserRepository` locates the row by username, so an incoming entity whose Id differs from, or is unset relative to, the stored one would retarget the tracked row's identity, leaving the guarantee "the stored identity is never overwritten by an edit" to caller convention instead of to the code.

`Lumina.DataAccess\Common\Persistence` already held `CollectionReconciler` and `EditableValuesCopier`, but there was no shared reference primitive and no explicit boundary defining what may live there, so the shared reference concern was expressed per type and per repository.

## Decision

Treat `Lumina.DataAccess\Common\Persistence` as a closed set of generic, entity-agnostic primitives that express the whole of aggregate graph persistence. Per-entity logic stays in the repository, and cross-cutting logic is generalized rather than specialized.

### The closed set

| Primitive | Responsibility |
|---|---|
| `EditableValuesCopier` | Copies only the editable scalar values onto the tracked entity. The primary key is never written, so the tracked identity always wins over the incoming one regardless of how the row was located, and the audit columns are never written, so the auditing interceptor remains their only writer. |
| `CollectionReconciler` | Reconciles a tracked child collection against a desired set by key selectors, preserving the identity and audit columns of the children that already exist. |
| `SharedReferenceResolver` | Resolves shared reference rows by name so each distinct name maps to a single tracked instance. The stored row wins over the incoming instance, and within-request duplicates are collapsed. |

### Genericity rules

- `EditableValuesCopier` is generic over `IAuditableEntity`. It reads the primary key from the Entity Framework metadata, so it enforces the key-matching contract for any entity without per-entity code.
- `CollectionReconciler` is generic over existing item, incoming item, and key type.
- `SharedReferenceResolver` is generic over `ISharedReferenceEntity`, a marker interface implemented by the shared reference records (`TagEntity`, `GenreEntity`). This is the one concern that is shaped by a family of entities, expressed through the marker interface rather than through per-entity code, so a new shared reference type adds no new persistence type.

### Application

- Insert resolves shared references across the whole aggregate, then normalizes every collection of the aggregate to the resolved instances.
- Update reconciles the tracked shared reference collections in place, reusing the stored rows.
- Scalar edits go through `EditableValuesCopier`. The repository decides how the tracked row is located (by its primary key, or by a natural key such as the username), and the copier keeps that row's primary key and audit columns untouched, so the incoming entity never has to carry a matching key.
- Child collections are reconciled through `CollectionReconciler` by a stable key.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| Bounded surface | The folder holds exactly three generic types; new entity types and new shared reference types add no persistence type |
| Consistency | Every repository persists graphs the same way, with the same identity and audit semantics |
| Correctness | Audit columns are never overwritten by an edit, and shared reference rows are never tracked more than once per request |
| Identity preservation | The tracked primary key is never overwritten by an edit, even when the incoming entity carries a different, or unset, key, so locating a row by a natural key stays safe |
| Testability | The primitives are covered directly by unit tests, independently of any repository |
| Ownership | No new external dependency and no declarative engine; the toolkit depends only on the internal `ISharedReferenceEntity` marker, and the logic stays explicit and inspectable |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| The generic resolver depends on an entity keyed by `Name` and translated by EF Core | `SharedReferenceResolver` is generic over `ISharedReferenceEntity` and is exercised by both unit and integration tests, which prove the EF translation |
| A future concern is generalized before its shape is stable | The rule permits keeping a concern in the repository until it can actually be generalized, rather than extracting speculatively |
| Behavior change from applying `EditableValuesCopier` to the seven repositories | Covered by the DataAccess and Presentation.Api integration suites, which pass across the migrated repositories |
| The copier relies on Entity Framework metadata to find the primary key | Unit tests copy onto a tracked entity whose incoming key does not match, and assert that the stored identity and audit columns survive |

## Alternatives Considered

### 1. Keep graph persistence duplicated in each repository

Leave the shared reference handling, collection reconciliation, and scalar copy implemented per repository.

**Rejected**: the duplication and the drift are the problem this decision addresses. Shared reference handling in particular has a single general shape, and repeating it per repository is what allows the shared-table and audit-column defects above to reappear.

### 2. Declarative aggregate persistence profiles

Introduce a generic persistence engine driven by a per-aggregate profile describing scalar copy behavior, child collections, and shared lookups, reducing repositories to load and apply.

**Rejected**: it is effectively a small in-house ORM, with functional and ownership costs disproportionate to the problem. The closed generic toolkit achieves the stated goal, keeping graph persistence explicit, without a new abstraction layer to own.

### 3. Model tags and genres as owned value objects

Stop modeling shared references as a shared table keyed by name, and store them per aggregate, removing the duplicate-key-in-graph class of bug at the source.

**Rejected**: it requires a database schema change and changes query behavior for shared references, which is out of scope. The shared tables are kept, and the concern is handled where it belongs, in the persistence toolkit.
