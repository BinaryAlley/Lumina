# Architecture Decision Record (ADR): 0018 - Media Neutral Consumption History, Resume Points, and Derived Statistics

## Status

**Accepted** (2026-10-26)

## Context

Consumption is one concept across media kinds. A music track and a book already produce sessions today, and video, photo, podcast and audiobook catalogues are planned. The same facts apply to each kind: who consumed an item, when the session started and ended, how long it was actively consumed, how far it reached, and where it can be resumed. A separate model per kind would repeat the aggregate, table, endpoints and contracts for every catalogue.

The feature raises questions:

1. **One concept, many media kinds**: consuming a track, a book or a video is the same fact for statistics, deletion and visibility, so a history per kind would repeat the model, the endpoints and the contracts.
2. **Two lifecycles**: history is a session with a start, progress and a terminal state, while resuming is the latest position of an item. History can be deleted without losing the position, and completion deletes the position but keeps the history.
3. **Reference integrity across catalogues**: one table cannot declare a foreign key to the tables of several media kinds, and a history row must survive the deletion of the item it references.
4. **Derived statistics**: a stored counter would disagree with the history after every deletion and would have to be rebuilt whenever a counting rule changes.
5. **Library policy**: whether a position is remembered and when a session is complete are per-library settings, not fixed properties of a media kind.
6. **Privacy**: a regular user must never read or delete another user's history, while an administrator must be able to do both without a separate permission toggle.

## Decision

Use one `MediaConsumptionBoundedContext` that owns two independent write aggregates and one read model:

- `MediaConsumptionRecord` is the history of one consumption session.
- `MediaResumePoint` is the latest position of one user and item.
- `ConsumptionStatistics` is a read model computed from the records on every request.

The decision includes these rules:

- **History and resume are separate aggregates.** A record has its own identifier, status, start, activity and end times, consumed duration, latest position, latest progress fraction and consecutive page count. A resume point has its own identifier and the business key `(UserId, MediaKind, MediaItemId)`. The storage layer owns the unique index of that key. A record is never the source of a resume point, and a resume point is never part of the record.
- **The latest reported position wins.** `LastPosition` is replaced by every report that carries a position, and `LastProgressFraction` by every report. Both may move backwards. Seeking from 75 percent back to the beginning and continuing from there leaves the resume point at the new position, not at the highest position ever reached. No monotonic rule is applied to either field.
- **Consumed duration is monotonic within one record.** It measures active listening, watching or reading time, it never decreases, and a report below the stored value is rejected. It is the only measurement with that rule.
- **Completion is explicit or threshold based.** A session stopped at or after the library completion threshold is recorded as `Completed` and its resume point is deleted. Completion never requires playback to the final second of a media file. A library that tracks progress without a completion threshold completes only when completion is reported explicitly.
- **Progress tracking and completion thresholds are per-library settings.** `ShouldTrackConsumptionProgress` and `CompletionThresholdPercentage` live in `LibraryConsumptionSettings` on the library aggregate. Defaults are applied by library type when a library is created, and when progress tracking is enabled later without a threshold. The per-library value always wins once it is set.
- **Statistics are derived, never stored.** `ConsumptionStatistics` aggregates the records in the application layer on every request. There is no rollup table, no projection table and no counter aggregate, so deleting a record changes the statistics immediately. The `MediaConsumptionRule` value objects own the counting rules per media kind.
- **Tags and genres are lowercase and shared across kinds.** `Tag` and `Genre` normalize names with `Trim().ToLowerInvariant()`, and the statistics group them by that normalized name across media kinds. The application resolves the terms of each consumed record through the catalogue repository of its media kind, in a per-kind branch, and the photo branch is added when the photo catalogue exists. For a music track, the terms fall back from the track to its album and then to its artist.
- **Access is based on the user and the administrator role.** A regular user reads and deletes only their own history, statistics and resume points. An administrator reads any user's history and statistics, and deletes any user's history and resume points. A resume point is readable only by its owner. There is no per-user permission toggle. Lifecycle writes (`start`, `progress`, `complete`, `stop`) are owner only, so an administrator does not write another user's session.

### Model

| Element | Decision |
|---|---|
| Bounded context | `MediaConsumptionBoundedContext` in the Domain layer. User, library and media item references are identifier copies under its `ExternalIdentifiers` directories, so the context depends on no other bounded context. |
| `MediaConsumptionRecord` | The write aggregate of a session. It holds a `MediaConsumptionRecordId`, the owner, the library, the media kind, the media item identifier, the status, the start, last activity and end times, the consumed duration, the latest position and progress fraction, and the consecutive page count. No album, artist, series or catalogue hierarchy is stored on the record. |
| `MediaResumePoint` | The write aggregate of the latest position. It holds a `MediaResumePointId`, the owner, the library, the media kind, the media item identifier, the latest position, the latest progress fraction and the last consumption time. It is created only for a resumable kind whose library tracks progress. |
| `ConsumptionStatistics` | A read model, not an aggregate and not a table. It is built from `MediaConsumptionRecordEntity` collections in the application handlers and mapped to the summary, recent, most consumed, tag and genre responses. |
| `MediaKind` | The shared enumeration in `Lumina.Domain.SharedKernel`. `MusicTrack` and `Book` are supported by the current catalogues. `PodcastEpisode`, `Audiobook`, `Movie`, `TvEpisode`, `Photo` and `Video` have rules and are listed for clients, but are rejected by the lifecycle endpoints until their catalogues exist. `None` is invalid. |
| `MediaConsumptionRule` | The shared value object hierarchy that owns resumability (`PositionKind`, `CanResume`), one-shot behavior and counting (`Counts`). It is reused by both aggregates and the statistics read model, so callers never switch on `MediaKind` for these rules. |
| Completion settings | `LibraryConsumptionSettings` on the `Library` aggregate. It stores `ShouldTrackConsumptionProgress` and an optional `CompletionThresholdPercentage`. The validator rejects a threshold when tracking is off, and a threshold outside 0 to 100 when it is supplied. |
| Storage | `MediaConsumptionRecords` and `MediaResumePoints`. The owner is a cascading foreign key to `Users`; the media item is a plain identifier with no foreign key to a media catalogue. Both tables carry an optimistic concurrency version and the audit columns. |
| Invariants | The resume points have a unique index on `(UserId, MediaKind, MediaItemId)`. Records are indexed by `(UserId, MediaKind, MediaItemId, LastActivityOnUtc)` for the history lists and by `(UserId, MediaKind, StartedOnUtc)` for date filtered statistics. |
| Domain events | `MediaConsumptionStartedDomainEvent`, `MediaConsumptionProgressedDomainEvent`, `MediaConsumptionCompletedDomainEvent` and `MediaConsumptionStoppedDomainEvent`. Resume point changes raise no event; the history events are the published seam. |

### Counting rules

The statistics count a session when the rule of its media kind says so. Duration totals every record, including the records that do not count, because the time was consumed regardless.

| Media kind | A session counts when |
|---|---|
| `MusicTrack` | The session completed, or the consumed duration reached 30 seconds |
| `PodcastEpisode` | The session completed, or the consumed duration reached 30 seconds |
| `Audiobook` | The session completed, or the consumed duration reached 60 seconds |
| `Movie` | The session completed, or the consumed duration reached 3 minutes |
| `TvEpisode` | The session completed, or the consumed duration reached 3 minutes |
| `Book` | The session completed, or more than 5 consecutive pages were read |
| `Photo` | Every session counts |
| `Video` | The session completed, or the consumed duration reached 3 minutes |

### Operations

- **Start**: validates the request, requires the current user, evaluates the library ownership policy, rejects an unsupported kind, resolves the item through the catalogue repository of its kind within the route library, and loads the library settings. When tracking is on and the kind can resume, the new record is initialized from the stored resume point; the consumed duration always starts at zero. A one-shot kind such as a photo is created completed in the same call.
- **Progress**: loads the record of the current user, applies the latest position, fraction, duration and consecutive page count, and upserts the resume point when tracking is on and the kind can resume. Progress never completes a session.
- **Complete**: loads the record of the current user, makes it terminal as `Completed`, and deletes the resume point for the same user, kind and item.
- **Stop**: loads the record and the library settings. It stops the record without a threshold check when tracking is off or no threshold is set. When tracking is on and a threshold is set, a final fraction at or above it completes the record and deletes the resume point; otherwise the record is stopped and the resume point is upserted.
- **Statistics**: the statistics repository binds the target user and every optional filter and returns read-only record entities. The application handlers aggregate the records, resolve display data and tags or genres through the catalogue repositories, paginate, and map the DTOs.
- **Deletion**: deleting a record, every record of an item, or the whole history is idempotent and never deletes resume points. Clearing resume points never deletes history. An administrator may delete any user's history or resume points through the per-user routes; reading a resume point stays owner only.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| One model | Music, books and every planned kind share one history, one resume representation, one set of lifecycle endpoints and one statistics read model |
| Context isolation | Both aggregates hold identifiers only, so the context depends on no media catalogue and could move to its own service |
| Independent lifecycles | History and resume can be deleted, completed and queried independently without one becoming the source of truth for the other |
| Correct statistics | Statistics always agree with the stored history, and a deleted record or a changed counting rule takes effect on the next request |
| Per-library policy | Progress tracking and completion thresholds are chosen per library instead of being hardcoded for a media kind |
| Privacy by default | A regular user cannot read or delete another user's data, and a wrong user id is reported as not authorized rather than revealing existence |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| Without a foreign key, a record can point at an item that no longer exists | This is intended: the history keeps its counts and duration, and the list returns the item as unavailable with blank display data so the user can delete it |
| A client can report a lower position after a higher one | This is the required behavior: the latest position wins, and only the consumed duration is monotonic |
| Statistics recompute from all matching records on every request | The repository filters by user, library, kind and date range in the query. A rollup can be added later as an optimization without changing the write model |
| A resume point can become orphaned when an item or library is deleted | Reading a resume point resolves the item through its catalogue first; a missing item or a library mismatch deletes the orphan and reports the item as not found |
| A forgotten `InProgress` record stays in history | The client ends a session on stop, skip, track change, reaching the end, a playback failure or unload. Pause and seek only report progress and leave the session in progress. A lost start is not expired by the server, so its record remains in history until the user deletes it. The limitation is accepted instead of adding expiry or cleanup of abandoned records |

## Alternatives Considered

### 1. Add a history and resume model per media kind

A `BookReadingHistory`, a `VideoWatchHistory` and a `PhotoViewHistory`, each with its own aggregate, table, endpoints and contracts.

**Rejected**: it repeats the same aggregate, table, endpoints and contracts for every kind, and it makes cross-kind statistics, deletion and admin visibility a union of several models. It also reintroduces the same extension cost for every planned catalogue.

### 2. Store the resume position inside the record

Keep the latest position only on `MediaConsumptionRecord` and find the last record when a client resumes.

**Rejected**: the history is a session log, while resuming is one current value per item. Finding it would mean reading the latest record and keeping abandoned records alive, and deleting history would silently remove the ability to resume. The two values have different lifetimes, business keys and deletion rules, so they are separate aggregates.

### 3. Store statistics as counter columns or a rollup table

Maintain total duration, counted sessions and completed sessions as counters, or project them into a table.

**Rejected**: a counter is a second source of truth. Deleting history, changing a counting rule or fixing a bad session would leave it wrong until a rebuild, while deriving the statistics from the records is always correct and needs no rebuild.

### 4. Hardcode resumability and completion thresholds per media kind

Decide in the aggregate that music can never resume, that books always complete at 100 percent, and that video always completes at 90 percent.

**Rejected**: users want to resume a movie without watching the credits to the last second, some music libraries want to resume, and a book library may want an explicit completion only. The library owns these choices, and the media kind only supplies the default and the shape of a position.
