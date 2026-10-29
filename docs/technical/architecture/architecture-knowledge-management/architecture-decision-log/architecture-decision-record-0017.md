# Architecture Decision Record (ADR): 0017 - One Generic Favorites Aggregate Keyed by Media Kind and Item Id

## Status

**Accepted** (2026-10-21)

## Context

A user must be able to mark any media item as a favorite: today the artists, albums and tracks of a music library and the books of a book library, and later videos and photos. Playlists already reference media items across bounded contexts by storing a media kind and a media item Id without a foreign key.

Favorites raise the same question with different constraints:

1. **One concept, many contexts**: a favorite is the same fact for every media kind, so a table and an aggregate per media kind would repeat one model four times now, and again for every media kind that is added.
2. **Bounded context isolation**: as specified by DDD, an aggregate does not reference objects of another bounded context, only identifiers placed in `ExternalIdentifiers` directories. A favorites aggregate that held an `Artist`, a `Book` or any other media entity would couple favorites to every media context.
3. **Referential integrity across kinds**: one table cannot declare a foreign key to the tables of several media kinds. A favorite must also outlive the item it references, because a library scan or a manual delete can remove an item, and the owner of the favorite has to be able to see and remove it afterwards instead of the favorite silently disappearing.
4. **Concurrent toggles**: a favorite toggle is rendered on many pages, and a user can have several tabs open, so the same add can arrive twice at nearly the same moment.
5. **Access control**: favoriting an item must not reveal whether an item exists in a library the user cannot access, and listing must not expose the display data of such a library.

## Decision

Store favorites as one generic `Favorite` aggregate in `FavoriteBoundedContext`, in one `Favorites` table. The table has a surrogate `FavoriteId` primary key and a unique business key `(UserId, MediaKind, MediaItemId)`. A media item is referenced only by its `FavoriteMediaKind` and the external `MediaItemId`; there is no foreign key to a media table. There are no per-kind favorite aggregates, tables, endpoints or contracts.

The decision includes these rules:

- **Add is idempotent.** Adding an item the user already favorited returns the existing favorite and raises no event.
- **Remove is idempotent.** Removing a favorite that is already absent succeeds without changing anything and raises no event.
- **The owner always comes from the authenticated principal.** No route, query or body carries the owner, and no administrator or library-owner override exposes another user's favorites.
- **Access is checked where it can leak.** Adding evaluates the library ownership policy of the item's library. Listing evaluates it for each distinct library in the returned page and hides the display data of inaccessible libraries. Removing and checking the status read only the current user's own favorites, so they evaluate no library policy.
- **Dangling favorites stay visible.** A favorite whose item no longer exists, or whose library is no longer accessible, is returned as an unavailable item with empty display fields so its owner can see and remove it.
- **Reads use the shared pagination contract.** The repository accepts the shared `PaginationDataDto` and returns the shared `PaginatedResultDto<T>`, like every other paginated listing. When the request provides neither `currentPage` nor `perPage`, every matching favorite is returned in a single page; when a page is requested, the page defaults to `1` and the page size defaults to `200`. A page beyond the last one is clamped to the last page, and there is no separate maximum page size. The status check accepts 1 to 100 Ids of one kind per request.

### Model

| Element | Decision |
|---|---|
| Aggregate | `Favorite` is the root, with a `FavoriteId`, a `UserId`, a `MediaItemId`, a `FavoriteMediaKind` and the audit columns. It has no entities. The user and the media item are referenced only through the identifier types in the context's `ExternalIdentifiers` directories; the kind is the shared `FavoriteMediaKind` enumeration. The media item identifier is generic across artists, albums, tracks, books and the planned video and photo kinds. |
| Media kind | `FavoriteMediaKind` has `MusicArtist`, `MusicAlbum`, `MusicTrack`, `Book`, `Video` and `Photo`, and it is stored by name. `Favorite.IsSupported` is `true` for the first four, and creating a favorite of the planned `Video` and `Photo` kinds is rejected with the validation error `FavoriteMediaKindIsNotSupported`. A planned kind becomes supported by lifting the restriction, with no change to the storage schema. |
| Domain events | `FavoriteAdded` is raised when a favorite is created, and `FavoriteRemoved` when it is removed. |
| Storage | The `Favorites` table holds the Id, the owner, the media kind, the media item Id and the audit columns. The owner is a foreign key that cascades, so favorites leave with their user. The media item is not a foreign key. |
| Invariant | A unique index on `(UserId, MediaKind, MediaItemId)` makes the storage medium the owner of the rule "one favorite per user and item", and serves the status check that looks up a user's favorites among checked Ids. Indexes on `(UserId, CreatedOnUtc)` and `(UserId, MediaKind, CreatedOnUtc)` serve the listing of every kind and of one kind. |
| Display data | The repository resolves, per kind and with projections that do not materialize the entities, the library Id of an item and the generic display fields of a page of favorites. The display fields are the title, the collection Id and title, the creator Id and name, and the cover path. Each kind fills what it has, and "no value" is the empty Guid or the empty string. Adding a media kind adds one branch to each resolution and one entry to the enumeration, with no new table, endpoint or contract. |

### Operations

- **Add**: the handler validates, requires the current user, rejects an unsupported kind, resolves the library of the item, evaluates the library ownership policy and returns the existing favorite without an event when the user already favorited the item. Otherwise it creates the aggregate, inserts it, saves the unit of work, queues the aggregate's events and reads the row back. If the unique index rejects a concurrent add, the handler re-reads the existing favorite and returns it without an event.
- **Remove**: the handler validates, requires the current user and loads the favorite by user, kind and item. A missing favorite is a successful removal with no event. Otherwise it removes the aggregate, deletes the row, saves the unit of work and queues the removal event. A concurrent removal is treated as success; any other concurrency conflict is reported.
- **List**: the repository scopes the query to the current user inside the storage query and returns the paginated listing ordered by the time of favoriting, with the shared pagination metadata. The handler evaluates the library policy once per distinct library in the page and maps a favorite with missing or inaccessible display data to an unavailable item with empty display fields.
- **Status**: the repository scopes the check to the current user and returns the Ids, among the checked ones, that the user favorited. The response carries only those Ids.
- **Errors**: `FavoriteAlreadyExists` remains defined but no operation produces it. `FavoriteNotFound` is used only where a stored favorite cannot be read back; removal of an absent favorite succeeds. `FavoriteMediaKindIsNotSupported` is returned by Add for the planned kinds.

## Consequences

### Positive Outcomes

| Aspect | Benefit |
|---|---|
| One model | Multiple media kinds share one aggregate, one table, four operations and one set of contracts |
| Context isolation | The aggregate holds identifiers only, so favorites depend on no media bounded context and could move to their own service |
| Survives deletion | A favorite is not removed by deleting or rescanning its item, and its owner is shown an unavailable item that can be removed |
| Race safe | The storage unique index, not the existence check alone, guarantees a single favorite per user and item, and a concurrent duplicate add resolves to the existing favorite |
| No existence oracle | An inaccessible item and a missing item are indistinguishable when adding, and inaccessible display data is never listed |
| No schema change for new kinds | The kind is stored by name, so a planned kind only needs its restriction lifted and its two resolutions written |

### Risks and Mitigations

| Risk | Mitigation Strategy |
|---|---|
| Without a foreign key, rows can point at items that no longer exist | This is intended: the listing returns such rows as unavailable and removal never needs the item. Nothing relies on the item being present |
| The per-kind resolution is a switch in the repository, so a new kind could be added to the enumeration without a resolution | `GetMediaItemLibraryIdAsync` reports no library for a kind without a branch, so an add of it is reported as not found, and `Favorite.IsSupported` has to be extended together with the branch |
| The listing evaluates the library policy once per distinct library of a page | The policy is evaluated once per distinct library, not once per favorite, so the cost grows with the number of distinct libraries rather than with the number of favorites. A client that needs a bound requests a page size, like every other paginated listing |
| The display projections can drift from how the catalogues show their items | The projections reuse the same sources as the listings of each catalogue, so a change to how a catalogue shows an item has to be mirrored in the matching branch |

## Alternatives Considered

### 1. One favorites table and aggregate per media kind

A `FavoriteArtist`, `FavoriteAlbum`, `FavoriteTrack` and `FavoriteBook` aggregate, each with a real foreign key to its media table.

**Rejected**: it repeats the same model, endpoints and contracts per kind, and again for every planned kind. The foreign keys would also make the favorite die with its item, which removes the user's ability to see and remove it, and each aggregate would need to reference a media bounded context directly.

### 2. A flag on each media entity

An `IsFavorite` column on artists, albums, tracks and books.

**Rejected**: a favorite is private to a user, and a flag on a shared catalogue row cannot be. A join table per user would be needed anyway, which brings back the first alternative.

### 3. Reuse the playlist model with a system playlist

Store favorites as the entries of a special playlist per user.

**Rejected**: a playlist is an ordered list that allows duplicates and is positioned by the user, while a favorite is an unordered set with exactly one entry per item and a listing ordered by the time of favoriting. The playlist operations, the position keys and the entry limits do not apply, and the favorites of artists, albums and books are not playable entries.

### 4. Enforce uniqueness only in the handler

Check for an existing favorite and rely on it, with no unique index.

**Rejected**: two requests of the same user can both pass the check, so duplicates would be created under concurrency. The storage medium must own the invariant, and the handler's check is kept only to make the common repeated add idempotent without a failure.

### 5. Return a conflict when an item is already a favorite

Report `FavoriteAlreadyExists` instead of returning the existing favorite.

**Rejected**: a toggle rendered on several pages, and several open tabs, make a repeated add a normal event rather than an error. An idempotent add keeps the client simple, since it does not have to distinguish a created favorite from an existing one. `FavoriteAlreadyExists` remains defined in the favorites error set, but no operation produces it.

## Amendment: Explicit Default Media Kind (2026-10-29)

`FavoriteMediaKind` now declares `None = 0` as its first member and default value, so an unset kind is never mistaken for `MusicArtist`. The other members keep their order and now occupy values 1 to 6. `None` is not a favoritable kind: the application validators reject it with `FavoriteMediaKindIsInvalid`, `Favorite.IsSupported` reports it as unsupported, and no operation creates a favorite of it. A stored favorite whose kind was recorded as `None` is still rehydrated without failure, so the read paths remain tolerant of legacy or malformed rows. Because the kind is stored by name, no data migration is required.

