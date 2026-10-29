# Lumina API

- [Lumina API](#lumina-api)
  - [Library Statistics](#library-statistics)
    - [When a Session Counts](#when-a-session-counts)
    - [Get Recently Consumed](#get-recently-consumed)
      - [Get Recently Consumed Request](#get-recently-consumed-request)
      - [Get Recently Consumed Response](#get-recently-consumed-response)
    - [Get Most Consumed](#get-most-consumed)
      - [Get Most Consumed Request](#get-most-consumed-request)
      - [Get Most Consumed Response](#get-most-consumed-response)
    - [Get Consumption Statistics](#get-consumption-statistics)
      - [Get Consumption Statistics Request](#get-consumption-statistics-request)
      - [Get Consumption Statistics Response](#get-consumption-statistics-response)
    - [Get All Users Consumption Statistics](#get-all-users-consumption-statistics)
      - [Get All Users Consumption Statistics Request](#get-all-users-consumption-statistics-request)
      - [Get All Users Consumption Statistics Response](#get-all-users-consumption-statistics-response)
    - [Get Tag Consumption](#get-tag-consumption)
      - [Get Tag Consumption Request](#get-tag-consumption-request)
      - [Get Tag Consumption Response](#get-tag-consumption-response)
    - [Get Genre Consumption](#get-genre-consumption)
      - [Get Genre Consumption Request](#get-genre-consumption-request)
      - [Get Genre Consumption Response](#get-genre-consumption-response)
    - [Delete Consumption Record](#delete-consumption-record)
      - [Delete Consumption Record Request](#delete-consumption-record-request)
      - [Delete Consumption Record Response](#delete-consumption-record-response)
    - [Delete Consumption Item History](#delete-consumption-item-history)
      - [Delete Consumption Item History Request](#delete-consumption-item-history-request)
      - [Delete Consumption Item History Response](#delete-consumption-item-history-response)
    - [Clear Consumption History](#clear-consumption-history)
      - [Clear Consumption History Request](#clear-consumption-history-request)
      - [Clear Consumption History Response](#clear-consumption-history-response)
    - [Clear Resume Points](#clear-resume-points)
      - [Clear Resume Points Request](#clear-resume-points-request)
      - [Clear Resume Points Response](#clear-resume-points-response)
    - [Pagination](#pagination)
    - [Scoping and Authorization](#scoping-and-authorization)
    - [Private Responses](#private-responses)
    - [Errors](#errors)
    - [Concurrency](#concurrency)

## Library Statistics

The library statistics read the media consumption history and aggregate it in the application layer. They are media neutral: music and book sessions appear in the same lists and the same summary, and the planned kinds use the same routes when their catalogues exist. The statistics never read a stored counter. Every response is computed from the consumption records on the request, so deleting history changes the statistics immediately, and a changed counting rule applies to the whole history at once. The lifecycle that creates the records, and the read of one resume point, are documented in [Media Consumption](../../media-consumption/Api.MediaConsumption.md).

The consumed duration is the active listening, watching or reading time reported by the session. It is totalled over every recorded session, including the sessions that do not count towards the statistics, because the time was consumed regardless. The counted sessions, completed sessions, stopped sessions and completion rate use the counting rules below.

### When a Session Counts

A session counts towards the statistics when its media kind's rule says so:

| Media kind | A session counts when |
| --- | --- |
| `MusicTrack` | The session completed, or the consumed duration is at least 30 seconds |
| `PodcastEpisode` | The session completed, or the consumed duration is at least 30 seconds |
| `Audiobook` | The session completed, or the consumed duration is at least 60 seconds |
| `Movie` | The session completed, or the consumed duration is at least 3 minutes |
| `TvEpisode` | The session completed, or the consumed duration is at least 3 minutes |
| `Book` | The session completed, or more than 5 consecutive pages were read |
| `Photo` | Every session counts |
| `Video` | The session completed, or the consumed duration is at least 3 minutes |

A completed session always counts. A stopped session counts only when it reaches the rule of its kind. A book with 5 consecutive pages does not count, while one with 6 does. The counting rule is separate from the library completion threshold: a session can count as consumed without the library considering the item complete.

### Get Recently Consumed

#### Get Recently Consumed Request

```js
GET api/v1/users/me/consumption-history/recently-consumed?currentPage=1&perPage=50
Authorization: Bearer <token>
```

An administrator can read the history of another user by targeting the per-user route:

```js
GET api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-history/recently-consumed?currentPage=1&perPage=50
Authorization: Bearer <token>
```

| Query parameter | Meaning |
| --- | --- |
| `libraryId` | Optional. The Id of a library to restrict the history to. It must be a non-empty Guid. |
| `mediaKind` | Optional. The name of a media kind to restrict the history to, matched case insensitively. Only member names are accepted, so a numeric value is rejected even when it is a defined enumeration value. `None` is rejected. |
| `currentPage` | Optional. The one based page of distinct media items to return. |
| `perPage` | Optional. The number of distinct media items per page. |
| `fromUtc` | Optional. The inclusive start of the history date range, in UTC. A trailing zone designator is honored and converted to UTC, while a value without one is interpreted as UTC. |
| `toUtc` | Optional. The inclusive end of the history date range, in UTC. A trailing zone designator is honored and converted to UTC, while a value without one is interpreted as UTC. It must not be before `fromUtc`. |

The same query parameters apply to the per-user route. The route user is the target, and the access policy decides whether the caller may read it.

The records are grouped to one row per media kind, media item and library before ordering, so an item consumed many times appears once. The rows are ordered by the time of the latest activity, from the most recent, and ties are broken by media kind, media item Id and library Id.

#### Get Recently Consumed Response

```js
200 Ok
```

```json
{
  "data": [
    {
      "mediaKind": "MusicTrack",
      "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
      "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
      "isAvailable": true,
      "title": "The Show Must Go On",
      "collectionTitle": "Innuendo",
      "creatorName": "Queen",
      "coverPath": "/artwork/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/cover.jpg",
      "lastConsumedOnUtc": "2026-10-24T12:12:00.0000000Z",
      "countedSessionCount": 4,
      "completedSessionCount": 3,
      "consumedDurationInSeconds": 1424.5
    },
    {
      "mediaKind": "Book",
      "mediaItemId": "7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
      "libraryId": "5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f",
      "isAvailable": true,
      "title": "The Fellowship of the Ring",
      "collectionTitle": "",
      "creatorName": "J. R. R. Tolkien",
      "coverPath": "/artwork/books/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d/cover.jpg",
      "lastConsumedOnUtc": "2026-10-23T20:40:00.0000000Z",
      "countedSessionCount": 2,
      "completedSessionCount": 1,
      "consumedDurationInSeconds": 7320
    }
  ],
  "currentPage": 1,
  "perPage": 50,
  "count": 2,
  "numberOfPages": 1
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `mediaKind` | string | The kind of the consumed media item. |
| `mediaItemId` | Guid | The Id of the consumed media item. |
| `libraryId` | Guid | The Id of the library recorded with the sessions. |
| `isAvailable` | boolean | Whether the item still exists and its library is accessible to the current user. |
| `title` | string | The title of the item, or an empty string when it is not available. |
| `collectionTitle` | string | The album title of a track, or an empty string when there is none. Books have no collection title. |
| `creatorName` | string | The artist of a track, or the name of one of a book's authors chosen alphabetically, or an empty string when there is none. |
| `coverPath` | string? | The path of the cover art, or `null` when there is none. |
| `lastConsumedOnUtc` | DateTime | The time of the latest recorded activity of the item. |
| `countedSessionCount` | integer | The sessions of the item that count towards the statistics. |
| `completedSessionCount` | integer | The sessions of the item that completed. |
| `consumedDurationInSeconds` | number | The active duration consumed in the item over all of its sessions, in seconds. |

A deleted item, an item whose library was deleted, and an item in a library the current user can no longer access keep their identity, counts and duration, but are returned with `isAvailable` set to `false`, blank display fields and no cover. This lets the user see the history row and delete it. An item that moved to another library after it was consumed is also reported as unavailable for the recorded library, because the display data of the recorded library cannot be shown for it.

### Get Most Consumed

#### Get Most Consumed Request

```js
GET api/v1/users/me/consumption-history/most-consumed?currentPage=1&perPage=50
Authorization: Bearer <token>
```

An administrator can read the most consumed items of another user:

```js
GET api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-history/most-consumed?currentPage=1&perPage=50
Authorization: Bearer <token>
```

The query parameters are those of [Get Recently Consumed](#get-recently-consumed-request). The records are grouped to one row per media kind, media item and library, then ordered by counted sessions, then by consumed duration, then by the latest activity, all descending, with the media item Id, media kind and library Id as the tiebreakers.

#### Get Most Consumed Response

```js
200 Ok
```

The response has the same shape as [Get Recently Consumed Response](#get-recently-consumed-response), ordered from the most consumed.

### Get Consumption Statistics

#### Get Consumption Statistics Request

```js
GET api/v1/users/me/consumption-statistics?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-10-31T23:59:59Z
Authorization: Bearer <token>
```

An administrator can read the statistics of another user:

```js
GET api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-statistics?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-10-31T23:59:59Z
Authorization: Bearer <token>
```

| Query parameter | Meaning |
| --- | --- |
| `libraryId` | Optional. The Id of a library to restrict the statistics to. It must be a non-empty Guid. |
| `mediaKind` | Optional. The name of a media kind to restrict the statistics to, matched case insensitively. Only member names are accepted, so a numeric value is rejected even when it is a defined enumeration value. `None` is rejected. |
| `fromUtc` | Optional. The inclusive start of the date range, in UTC. A trailing zone designator is honored and converted to UTC, while a value without one is interpreted as UTC. |
| `toUtc` | Optional. The inclusive end of the date range, in UTC. A trailing zone designator is honored and converted to UTC, while a value without one is interpreted as UTC. It must not be before `fromUtc`. |

#### Get Consumption Statistics Response

```js
200 Ok
```

```json
{
  "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
  "totalConsumedDurationInSeconds": 12840.5,
  "countedSessionCount": 62,
  "completedSessionCount": 41,
  "stoppedSessionCount": 21,
  "completionRate": 0.6612903,
  "mediaKindSummaries": [
    {
      "mediaKind": "MusicTrack",
      "totalConsumedDurationInSeconds": 5520.5,
      "countedSessionCount": 60,
      "completedSessionCount": 40,
      "stoppedSessionCount": 20,
      "completionRate": 0.6666667
    },
    {
      "mediaKind": "Book",
      "totalConsumedDurationInSeconds": 7320,
      "countedSessionCount": 2,
      "completedSessionCount": 1,
      "stoppedSessionCount": 1,
      "completionRate": 0.5
    }
  ]
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `userId` | Guid | The user the statistics belong to. |
| `totalConsumedDurationInSeconds` | number | The consumed duration over every recorded session, in seconds. Sessions below the counting threshold are included. |
| `countedSessionCount` | integer | The sessions that count towards the statistics. |
| `completedSessionCount` | integer | The recorded sessions that completed. |
| `stoppedSessionCount` | integer | The counted sessions that stopped before completing. |
| `completionRate` | number | The completed sessions divided by the counted sessions, between 0 and 1, or 0 when no session counts. |
| `mediaKindSummaries` | array | The same fields, without `userId`, broken down by media kind, ordered by media kind. |

The overall totals are the sums of the per-kind summaries, so the completion rate never disagrees with the breakdown. When no record matches, every total is 0 and the breakdown is an empty array.

### Get All Users Consumption Statistics

#### Get All Users Consumption Statistics Request

```js
GET api/v1/users/consumption-statistics?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-10-31T23:59:59Z
Authorization: Bearer <token>
```

The same optional `libraryId`, `mediaKind`, `fromUtc` and `toUtc` filters apply. Only an administrator can call this route. It is the only statistics route that loads the records of every user.

#### Get All Users Consumption Statistics Response

```js
200 Ok
```

```json
{
  "total": {
    "totalConsumedDurationInSeconds": 12840.5,
    "countedSessionCount": 62,
    "completedSessionCount": 41,
    "stoppedSessionCount": 21,
    "completionRate": 0.6612903,
    "mediaKindSummaries": [
      {
        "mediaKind": "MusicTrack",
        "totalConsumedDurationInSeconds": 5520.5,
        "countedSessionCount": 60,
        "completedSessionCount": 40,
        "stoppedSessionCount": 20,
        "completionRate": 0.6666667
      },
      {
        "mediaKind": "Book",
        "totalConsumedDurationInSeconds": 7320,
        "countedSessionCount": 2,
        "completedSessionCount": 1,
        "stoppedSessionCount": 1,
        "completionRate": 0.5
      }
    ]
  },
  "users": [
    {
      "userId": "2cff0c0f-3a67-4e30-b94f-f4e948a3b64f",
      "totalConsumedDurationInSeconds": 12840.5,
      "countedSessionCount": 62,
      "completedSessionCount": 41,
      "stoppedSessionCount": 21,
      "completionRate": 0.6612903,
      "mediaKindSummaries": [
        {
          "mediaKind": "MusicTrack",
          "totalConsumedDurationInSeconds": 5520.5,
          "countedSessionCount": 60,
          "completedSessionCount": 40,
          "stoppedSessionCount": 20,
          "completionRate": 0.6666667
        },
        {
          "mediaKind": "Book",
          "totalConsumedDurationInSeconds": 7320,
          "countedSessionCount": 2,
          "completedSessionCount": 1,
          "stoppedSessionCount": 1,
          "completionRate": 0.5
        }
      ]
    }
  ]
}
```

`total` carries the same fields as a user summary without `userId`, including its own `mediaKindSummaries` array, which combines the per-kind summaries of every user. `users` holds one summary per user that has a matching record, ordered by user Id. The all-users route returns no history lists and no tag or genre breakdown, because those are only meaningful per user.

### Get Tag Consumption

#### Get Tag Consumption Request

```js
GET api/v1/users/me/consumption-statistics/tags?mediaKind=MusicTrack
Authorization: Bearer <token>
```

An administrator can read the tag breakdown of another user:

```js
GET api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-statistics/tags?mediaKind=MusicTrack
Authorization: Bearer <token>
```

The optional `libraryId`, `mediaKind`, `fromUtc` and `toUtc` filters are the same as for [Get Consumption Statistics](#get-consumption-statistics-request).

#### Get Tag Consumption Response

```js
200 Ok
```

```json
[
  {
    "name": "fantasy",
    "consumedItemCount": 2,
    "consumedDurationInSeconds": 7320
  },
  {
    "name": "horror",
    "consumedItemCount": 1,
    "consumedDurationInSeconds": 1424.5
  }
]
```

The records are grouped to one row per media kind, media item and library first, so an item with several sessions contributes its tags once, not once per session. A music track with no tags of its own contributes the tags of its album, and then the tags of its artist when the album has none. Only items in an accessible library with at least one counted session contribute. A name that appears twice on the same item contributes once. Names are normalized to lowercase, so `Horror` and `horror` share one bucket across media kinds. The rows are ordered by consumed item count, then consumed duration, both descending, with the name ascending as the tiebreaker. An item that no longer exists, or that moved to another library after it was consumed, does not contribute to the breakdown, because its terms cannot be resolved for the recorded library.

`consumedItemCount` is the number of distinct items that carry the tag, not the number of sessions. `consumedDurationInSeconds` is the consumed duration of those items, over all of their sessions.

### Get Genre Consumption

#### Get Genre Consumption Request

```js
GET api/v1/users/me/consumption-statistics/genres?mediaKind=Book
Authorization: Bearer <token>
```

An administrator can read the genre breakdown of another user:

```js
GET api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-statistics/genres?mediaKind=Book
Authorization: Bearer <token>
```

The optional `libraryId`, `mediaKind`, `fromUtc` and `toUtc` filters are the same as for [Get Consumption Statistics](#get-consumption-statistics-request).

#### Get Genre Consumption Response

```js
200 Ok
```

```json
[
  {
    "name": "fantasy",
    "consumedItemCount": 2,
    "consumedDurationInSeconds": 7320
  },
  {
    "name": "science fiction",
    "consumedItemCount": 1,
    "consumedDurationInSeconds": 1424.5
  }
]
```

The genre breakdown follows the same item level aggregation, lowercase normalization, ordering and availability rules as the tag breakdown. A music track with no genres of its own contributes the genres of its album, and then the genres of its artist when the album has none.

### Delete Consumption Record

#### Delete Consumption Record Request

```js
DELETE api/v1/users/me/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f
Authorization: Bearer <token>
```

An administrator can delete a record of another user:

```js
DELETE api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f
Authorization: Bearer <token>
```

| Route value | Meaning |
| --- | --- |
| `recordId` | The Id of the consumption record to delete. It must be a non-empty Guid. |
| `userId` | The owner of the record on the per-user route. An administrator can name any user; a regular user can only name themselves. |

The record is deleted hard. Deleting a record that does not exist, or that belongs to another user, succeeds without changing anything. The resume point of the item is not deleted, because the history and the resume point are separate actions.

#### Delete Consumption Record Response

```js
200 Ok
```

The response body is an empty JSON object.

### Delete Consumption Item History

#### Delete Consumption Item History Request

```js
DELETE api/v1/users/me/consumption-history/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a
Authorization: Bearer <token>
```

An administrator can delete every record of an item of another user:

```js
DELETE api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-history/media-items/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d/5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f
Authorization: Bearer <token>
```

| Route value | Meaning |
| --- | --- |
| `mediaKind` | The kind of the item. `None` is rejected. |
| `mediaItemId` | The Id of the item. It must be a non-empty Guid. |
| `libraryId` | The Id of the library recorded with the item. It must be a non-empty Guid. |
| `userId` | The owner of the history on the per-user route. An administrator can name any user; a regular user can only name themselves. |

Every recorded session of the item in that library is deleted, not only the sessions inside a date range. The deletion is idempotent and leaves the resume point untouched.

#### Delete Consumption Item History Response

```js
200 Ok
```

The response body is an empty JSON object.

### Clear Consumption History

#### Clear Consumption History Request

```js
DELETE api/v1/users/me/consumption-history
Authorization: Bearer <token>
```

An administrator can clear the whole history of another user:

```js
DELETE api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/consumption-history
Authorization: Bearer <token>
```

Every consumption record of the target user is deleted. The deletion is idempotent and does not delete any resume point, so the continue watching list survives until the user removes it separately.

#### Clear Consumption History Response

```js
200 Ok
```

The response body is an empty JSON object.

### Clear Resume Points

#### Clear Resume Points Request

```js
DELETE api/v1/users/me/media-resume-points?libraryId=3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a
Authorization: Bearer <token>
```

An administrator can clear the resume points of another user:

```js
DELETE api/v1/users/2cff0c0f-3a67-4e30-b94f-f4e948a3b64f/media-resume-points
Authorization: Bearer <token>
```

| Query parameter | Meaning |
| --- | --- |
| `libraryId` | Optional. The Id of a library to restrict the deletion to. It must be a non-empty Guid. When it is omitted, every resume point of the user is deleted. |

Clearing resume points deletes no history. The operation is idempotent.

#### Clear Resume Points Response

```js
200 Ok
```

The response body is an empty JSON object.

### Pagination

The recently consumed and most consumed routes accept `currentPage` and `perPage`:

- When both are omitted, every matching item is returned in one page. `currentPage` is 1, `perPage` is the number of items, which is 0 when none match, and `numberOfPages` is 1.
- When either is supplied, the other defaults to 1 for `currentPage` and 50 for `perPage`.
- A page below 1 is treated as 1. A page size below 1 is treated as 1.
- When at least one item matches, a page beyond the last one is clamped to the last page.
- When a requested page matches no item, `count` is 0, `currentPage` is 1 and `numberOfPages` is 0.
- `count` is the number of distinct items that match the filters, before pagination.

The statistics, tag and genre routes are not paginated, because their result is already an aggregate bounded by the number of media kinds, tags or genres.

### Scoping and Authorization

Every route in this document requires authentication.

- The `/users/me/...` routes always act on the current user. The identity comes from the authenticated principal, never from the route, query or body.
- The `/users/{userId}/...` routes act on the named user. A regular user may name only themselves. Naming another user returns `403 NotAuthorized`, exactly like a route that names no accessible user, so the response cannot be used to find out whether a user or their history exists.
- The `/users/consumption-statistics` route is only for administrators. A regular user receives `403 NotAuthorized`.
- Administrator access is role based. The per-user routes evaluate the consumption activity access policy, which allows the owner or an administrator. The all-users route checks the administrator role directly. There is no separate permission toggle for consumption activity.

The statistics and lists never expose the history of another user to a regular user. Display data is resolved per distinct library through the library ownership policy. An item in a library the current user can no longer access keeps its counts but has blank display data, so the history lists reveal no inaccessible title, collection, creator or cover. The service wide totals never include the records of a user that the caller cannot target.

Deleting a record, every record of an item, the whole history, or the resume points belongs to the same access rule. An administrator can delete any user's consumption activity through the per-user routes, and a regular user can delete only their own. Reading a resume point is not part of this surface; it stays owner only, as documented in [Media Consumption](../../media-consumption/Api.MediaConsumption.md).

### Private Responses

Every response produced by these endpoints is private to the authenticated user, and each endpoint sets:

- `Cache-Control: private, no-store`
- `Vary: Authorization`

The headers are set before the operation runs, so they are present on the endpoint's problem responses too. A request rejected by the authentication middleware with `401` before the endpoint runs is produced by the framework rather than by the endpoint.

### Errors

Errors are returned as problem details. A request without a valid token is rejected with `401 Unauthorized` before it reaches the operation.

| Status | Error | When |
| --- | --- | --- |
| `401` | | The request carries no token, or an invalid or expired one. |
| `403` | `NotAuthorized` | The authenticated principal carries no user identity, a regular user targets another user, or a regular user calls the all-users route. |
| `409` | `ConcurrencyConflict` | A deletion could not be stored because the database is busy or locked, or because another request changed the same row after this request read it. Repeating the request is safe. |
| `422` | see below | The request failed validation. |

A request that fails validation is rejected with `422 Unprocessable Entity` and a validation problem whose `errors` object lists the validation error names under the generic `General.Validation` code.

| Error | Operation | When |
| --- | --- | --- |
| `TargetUserIdIsInvalid` | Per-user routes | The `userId` route value is not a non-empty Guid. |
| `ConsumptionRecordIdCannotBeEmpty` | Delete record | The `recordId` route value is not a non-empty Guid. |
| `MediaItemIdCannotBeEmpty` | Delete item history | The `mediaItemId` route value is the empty Guid. |
| `LibraryIdCannotBeEmpty` | Statistics filters, delete item history, clear resume points | A supplied library Id is not a non-empty Guid. |
| `UnsupportedMediaKind` | Statistics filters, delete item history | The supplied kind is `None` or is not a defined media kind. Omitting `mediaKind` is valid on the statistics filters. |
| `ConsumptionDateRangeIsInvalid` | Statistics filters | A date bound is not a valid date, or `fromUtc` is after `toUtc`. |

A value that cannot be bound to the request type, such as a malformed route Id on delete item history, is rejected by the framework before these validators run. Media kind query filters are carried as text, so an unknown name reaches the validator and is reported as `UnsupportedMediaKind`.

### Concurrency

Deletions use the optimistic concurrency version stored on the consumption record and resume point entities. A deletion that races with another change is reported as `409 ConcurrencyConflict`, and a busy or locked SQLite write is reported the same way. Deleting an already absent record, item history or set of resume points succeeds, so a retried deletion is safe.
