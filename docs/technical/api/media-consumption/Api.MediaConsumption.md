# Lumina API

- [Lumina API](#lumina-api)
  - [Media Consumption](#media-consumption)
    - [Start Consumption](#start-consumption)
      - [Start Consumption Request](#start-consumption-request)
      - [Start Consumption Response](#start-consumption-response)
    - [Get Consumption Record](#get-consumption-record)
      - [Get Consumption Record Request](#get-consumption-record-request)
      - [Get Consumption Record Response](#get-consumption-record-response)
    - [Report Consumption Progress](#report-consumption-progress)
      - [Report Consumption Progress Request](#report-consumption-progress-request)
      - [Report Consumption Progress Response](#report-consumption-progress-response)
    - [Complete Consumption](#complete-consumption)
      - [Complete Consumption Request](#complete-consumption-request)
      - [Complete Consumption Response](#complete-consumption-response)
    - [Stop Consumption](#stop-consumption)
      - [Stop Consumption Request](#stop-consumption-request)
      - [Stop Consumption Response](#stop-consumption-response)
    - [Get Resume Point](#get-resume-point)
      - [Get Resume Point Request](#get-resume-point-request)
      - [Get Resume Point Response](#get-resume-point-response)
    - [Media Kinds](#media-kinds)
    - [Session Statuses](#session-statuses)
    - [Position Kinds](#position-kinds)
    - [Reportable Ranges](#reportable-ranges)
    - [Session Lifecycle and Resume Points](#session-lifecycle-and-resume-points)
    - [Completion Threshold](#completion-threshold)
    - [Scoping and Authorization](#scoping-and-authorization)
    - [Private Responses](#private-responses)
    - [Errors](#errors)
    - [Concurrency](#concurrency)

## Media Consumption

The media consumption API records what the current user consumed, how far they reached, how long they consumed it, and where they can resume it. It is media neutral: the same routes serve a music track and a book, and every future catalogue uses the same routes without a per-kind endpoint. The statistics read these records; see [Library Statistics](../library/statistics/Api.LibraryStatistics.md) for the counting rules and the aggregated reads.

A consumption session is stored as a `MediaConsumptionRecord`. It carries the kind of media, the library and the item, the start and end times, the latest position and progress fraction, the active consumed duration, and the number of consecutive pages read for page based media. It is not a playback session: it stores no queue, no current index, no repeat or shuffle state, and it never restores playback.

A resume point is stored separately as a `MediaResumePoint`, with at most one row per user, media kind and media item. It carries the last known position, the last progress fraction, and when the item was last consumed. Completion deletes it. A session that stops without completing can keep it, depending on the library settings.

All the routes in this document require authentication and act on the current user. No route or body accepts a user Id. The lifecycle writes are owner only: an admin cannot progress, complete or stop another user's session.

The supported media kinds are `MusicTrack` and `Book`. The other kinds are planned and are rejected until their catalogues exist, see [Media Kinds](#media-kinds).

All responses are private to the authenticated user, including the endpoint problem responses, see [Private Responses](#private-responses).

### Start Consumption

#### Start Consumption Request

```js
POST api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records
Authorization: Bearer <token>
```

The same operation starts a book session:

```js
POST api/v1/libraries/5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f/media-items/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d/consumption-records
Authorization: Bearer <token>
```

| Route value | Meaning |
| --- | --- |
| `libraryId` | The Id of the media library that contains the item. It must be a Guid and cannot be the empty Guid. |
| `mediaKind` | The kind of media, matched case insensitively. Only the member names are accepted: `MusicTrack` and `Book` for now. Numeric values such as `1`, `6` or `0` are rejected, even when they are defined enumeration values. |
| `mediaItemId` | The Id of the consumed item. It must be a Guid and cannot be the empty Guid. |

Starts a consumption session for the item named by the route. The request has no body: every identifier is taken from the route, so a client cannot substitute the library, kind or item with a body value.

The operation validates the request, requires the current user, and evaluates the library ownership policy for the route library. The library is then loaded, and the item is resolved through the catalogue repository of its kind. The item must exist and must belong to the route library. A missing item, an item that belongs to another library, and an item of another kind are reported with the same kind specific not found error, so the route cannot be used to probe another library or another kind.

When the library tracks progress and the kind can resume, the new session starts from the stored resume point: the record's `lastPosition` and `lastProgressFraction` are initialized from it, and `consecutivePagesRead` starts at zero. The consumed duration always starts at zero, because it measures this session only. When tracking is off, or when there is no stored resume point, the session starts with a null position and a zero progress fraction.

The start operation records the session immediately and returns its identifier. A new session of a one-shot kind such as a photo would be created completed in the same call, but one-shot kinds are not supported by the current catalogues, so they are rejected before that branch is reached.

#### Start Consumption Response

```js
200 Ok
```

A music track with no stored resume point:

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "mediaKind": "MusicTrack",
  "status": "InProgress",
  "startedOnUtc": "2026-10-24T12:00:00.0000000Z",
  "lastActivityOnUtc": "2026-10-24T12:00:00.0000000Z",
  "endedOnUtc": null,
  "consumedDurationInSeconds": 0,
  "lastPosition": null,
  "lastProgressFraction": 0,
  "consecutivePagesRead": 0
}
```

A book with a stored resume point:

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "libraryId": "5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f",
  "mediaItemId": "7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "mediaKind": "Book",
  "status": "InProgress",
  "startedOnUtc": "2026-10-24T12:00:00.0000000Z",
  "lastActivityOnUtc": "2026-10-24T12:00:00.0000000Z",
  "endedOnUtc": null,
  "consumedDurationInSeconds": 0,
  "lastPosition": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/4[chapter-3]!/4/2/2)"
  },
  "lastProgressFraction": 0.42,
  "consecutivePagesRead": 0
}
```

The returned `id` identifies the session in the get, progress, complete and stop routes.

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | Guid | The Id of the consumption record. |
| `libraryId` | Guid | The Id of the library that contains the consumed item. |
| `mediaItemId` | Guid | The Id of the consumed item. |
| `mediaKind` | string | The kind of media: `MusicTrack` or `Book` today. |
| `status` | string | The state of the session: `InProgress`, `Completed` or `Stopped`. |
| `startedOnUtc` | DateTime | When the session started. |
| `lastActivityOnUtc` | DateTime | When the session last received a progress, complete or stop request. |
| `endedOnUtc` | DateTime? | When the session completed or stopped, or `null` while it is in progress. |
| `consumedDurationInSeconds` | number | The active duration consumed by the session, in seconds. It starts at zero and never decreases. |
| `lastPosition` | object? | The latest known position, or `null` when there is none. See [Position Kinds](#position-kinds). |
| `lastProgressFraction` | number | The latest reported progress fraction, from 0 to 1. It may move backwards. |
| `consecutivePagesRead` | integer | The number of consecutive pages read in this session. It is zero for every kind except page based media such as books. |

The start response is `200 Ok`. A wrong-library item and a missing item return the same `TrackNotFound` or `BookNotFound` error. A missing library returns `LibraryNotFound`. Starting in a library the current user cannot access returns `NotAuthorized`.

### Get Consumption Record

#### Get Consumption Record Request

```js
GET api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f
Authorization: Bearer <token>
```

| Route value | Meaning |
| --- | --- |
| `recordId` | The Id of the consumption record. It must be a Guid and cannot be the empty Guid. |

Returns the consumption record identified by the route, when it belongs to the current user. A record of another user is reported as not found, exactly like a record that does not exist, so a guessed identifier never reveals the existence of another user's session.

#### Get Consumption Record Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "libraryId": "5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f",
  "mediaItemId": "7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "mediaKind": "Book",
  "status": "Stopped",
  "startedOnUtc": "2026-10-24T12:00:00.0000000Z",
  "lastActivityOnUtc": "2026-10-24T12:12:00.0000000Z",
  "endedOnUtc": "2026-10-24T12:12:00.0000000Z",
  "consumedDurationInSeconds": 720,
  "lastPosition": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/4[chapter-3]!/4/2/2)"
  },
  "lastProgressFraction": 0.42,
  "consecutivePagesRead": 6
}
```

The response is a `ConsumptionRecordResponse` with the fields listed in [Start Consumption Response](#start-consumption-response). A record of another user, and a record that does not exist, are both reported with `ConsumptionRecordNotFound`.

### Report Consumption Progress

#### Report Consumption Progress Request

```js
PUT api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/progress
Authorization: Bearer <token>
Content-Type: application/json
```

A music track reports a time offset:

```json
{
  "position": {
    "kind": "TimeOffset",
    "timeOffsetSeconds": 92.5,
    "location": null
  },
  "progressFraction": 0.31,
  "consumedDurationInSeconds": 92.5,
  "consecutivePagesRead": 0
}
```

A book reports an opaque location and the number of consecutive pages read:

```json
{
  "position": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/6[chapter-2]!/4/2/2)"
  },
  "progressFraction": 0.18,
  "consumedDurationInSeconds": 300,
  "consecutivePagesRead": 2
}
```

| Route value | Meaning |
| --- | --- |
| `recordId` | The Id of the consumption record, taken from the route. It must be a Guid and cannot be the empty Guid. |

| Body field | Type | Required | Meaning |
| --- | --- | --- | --- |
| `position` | object | no | The latest position reported by the client. Send a complete position object to update it; omit it, or send an empty object, to keep the stored position. |
| `progressFraction` | number | yes | The fraction of the item the client reached, from 0 to 1, finite. It may move backwards. |
| `consumedDurationInSeconds` | number | yes | The active duration consumed by this session, in seconds. It must be finite, non-negative, within the maximum reportable range, and never lower than the value already stored for the session. |
| `consecutivePagesRead` | integer | no | The number of consecutive pages read in this session. It must be non-negative and is only accepted for page based media. Defaults to 0. |

Reports the latest measurements of an in-progress session of the current user. The position and the progress fraction are the latest reported values, not the maximum values reached: a seek backwards is accepted and is stored as the new position. The consumed duration is the only monotonic measurement of the session and can never decrease. The operation never completes a session, even when the reported fraction reaches 1, because completion is explicit or threshold based.

A session of a non-page-based kind cannot report consecutive pages above zero. A music client that sends `consecutivePagesRead` above zero is rejected with `ConsecutivePagesReadIsOnlySupportedForPageBasedMedia`, even though the request contract has the field for every kind.

A book page count of 5 does not count as a session for statistics, while 6 does, but that counting rule belongs to the statistics read model and does not change the stored progress.

The book reader counts a page as one visible reader viewport. A PDF page is a section of its own, while a reflowable section can hold several viewport pages, so the reported page count follows the pages the reader actually scrolled through rather than the printed pages of the source book.

#### Report Consumption Progress Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "libraryId": "5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f",
  "mediaItemId": "7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "mediaKind": "Book",
  "status": "InProgress",
  "startedOnUtc": "2026-10-24T12:00:00.0000000Z",
  "lastActivityOnUtc": "2026-10-24T12:05:00.0000000Z",
  "endedOnUtc": null,
  "consumedDurationInSeconds": 300,
  "lastPosition": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/6[chapter-2]!/4/2/2)"
  },
  "lastProgressFraction": 0.18,
  "consecutivePagesRead": 2
}
```

The response is a `ConsumptionRecordResponse` with the fields listed in [Start Consumption Response](#start-consumption-response). A record of another user, and a record that does not exist, are both reported with `ConsumptionRecordNotFound`. A terminal session returns `ConsumptionIsNotInProgress`.

### Complete Consumption

#### Complete Consumption Request

```js
POST api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/complete
Authorization: Bearer <token>
Content-Type: application/json
```

A music track reports its final time offset:

```json
{
  "position": {
    "kind": "TimeOffset",
    "timeOffsetSeconds": 300,
    "location": null
  },
  "progressFraction": 1,
  "consumedDurationInSeconds": 300
}
```

A book reports its final opaque location:

```json
{
  "position": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/8[chapter-4]!/4/2/2)"
  },
  "progressFraction": 1,
  "consumedDurationInSeconds": 900
}
```

| Route value | Meaning |
| --- | --- |
| `recordId` | The Id of the consumption record, taken from the route. It must be a Guid and cannot be the empty Guid. |

| Body field | Type | Required | Meaning |
| --- | --- | --- | --- |
| `position` | object | no | The final position. It is stored when provided; otherwise the stored position is kept. |
| `progressFraction` | number | yes | The final fraction of the item the client reached, from 0 to 1, finite. |
| `consumedDurationInSeconds` | number | yes | The final active duration consumed by the session, in seconds. It must be finite, non-negative, within the maximum reportable range, and never lower than the value already stored. |

Completes an in-progress session of the current user. Completion is explicit, so it is recorded even when the media was not played to its final second. Completing a session deletes the resume point of the current user for the same media item. The final position, progress fraction, consumed duration and consecutive pages remain on the record.

#### Complete Consumption Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "mediaKind": "MusicTrack",
  "status": "Completed",
  "startedOnUtc": "2026-10-24T12:00:00.0000000Z",
  "lastActivityOnUtc": "2026-10-24T12:05:00.0000000Z",
  "endedOnUtc": "2026-10-24T12:05:00.0000000Z",
  "consumedDurationInSeconds": 300,
  "lastPosition": {
    "kind": "TimeOffset",
    "timeOffsetSeconds": 300,
    "location": null
  },
  "lastProgressFraction": 1,
  "consecutivePagesRead": 0
}
```

The response is a `ConsumptionRecordResponse` with the fields listed in [Start Consumption Response](#start-consumption-response). A record of another user, and a record that does not exist, are both reported with `ConsumptionRecordNotFound`. A terminal session returns `ConsumptionIsAlreadyTerminal`.

### Stop Consumption

#### Stop Consumption Request

```js
POST api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop
Authorization: Bearer <token>
Content-Type: application/json
```

A music track reports where it was stopped:

```json
{
  "position": {
    "kind": "TimeOffset",
    "timeOffsetSeconds": 60,
    "location": null
  },
  "progressFraction": 0.6,
  "consumedDurationInSeconds": 60
}
```

A book reports where it was stopped:

```json
{
  "position": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/6[chapter-2]!/4/2/2)"
  },
  "progressFraction": 0.18,
  "consumedDurationInSeconds": 300
}
```

| Route value | Meaning |
| --- | --- |
| `recordId` | The Id of the consumption record, taken from the route. It must be a Guid and cannot be the empty Guid. |

| Body field | Type | Required | Meaning |
| --- | --- | --- | --- |
| `position` | object | no | The final position. It is stored when provided; otherwise the stored position is kept. |
| `progressFraction` | number | yes | The final fraction of the item the client reached, from 0 to 1, finite. |
| `consumedDurationInSeconds` | number | yes | The final active duration consumed by the session, in seconds. It must be finite, non-negative, within the maximum reportable range, and never lower than the value already stored. |

Stops an in-progress session of the current user before it completes. The library settings decide the terminal status:

- When the library tracks progress, has a completion threshold, and the final progress fraction is at or above that threshold, the session is recorded as `Completed` instead of `Stopped`, and its resume point is deleted. Completion does not require the media to reach its final second.
- When the library tracks progress without a completion threshold, the session is recorded as `Stopped`.
- When the library does not track progress, the session is recorded as `Stopped`, without a completion threshold check.

A stopped session keeps a resume point only when the library tracks progress, the kind can resume, and a final position is known. Without a known position there is nothing to resume from, so no resume point is written. Music libraries default to tracking off, so a normal music stop records history but does not create a resume point. A music library that explicitly enables tracking can create one.

#### Stop Consumption Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "mediaItemId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "mediaKind": "MusicTrack",
  "status": "Stopped",
  "startedOnUtc": "2026-10-24T12:00:00.0000000Z",
  "lastActivityOnUtc": "2026-10-24T12:01:00.0000000Z",
  "endedOnUtc": "2026-10-24T12:01:00.0000000Z",
  "consumedDurationInSeconds": 60,
  "lastPosition": {
    "kind": "TimeOffset",
    "timeOffsetSeconds": 60,
    "location": null
  },
  "lastProgressFraction": 0.6,
  "consecutivePagesRead": 0
}
```

The response is a `ConsumptionRecordResponse` with the fields listed in [Start Consumption Response](#start-consumption-response). The `status` is `Completed` when the stop reached the library completion threshold, and `Stopped` otherwise. A record of another user, and a record that does not exist, are both reported with `ConsumptionRecordNotFound`. A terminal session returns `ConsumptionIsAlreadyTerminal`.

### Get Resume Point

#### Get Resume Point Request

```js
GET api/v1/users/me/media-resume-points/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d
Authorization: Bearer <token>
```

The same operation reads a music track resume point, when the music library tracks progress:

```js
GET api/v1/users/me/media-resume-points/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
Authorization: Bearer <token>
```

| Route value | Meaning |
| --- | --- |
| `mediaKind` | The kind of media to resume, matched case insensitively. `MusicTrack` and `Book` can resume today. `Photo` cannot resume. |
| `mediaItemId` | The Id of the media item to resume. It must be a Guid and cannot be the empty Guid. |

Returns the resume point of the current user for the item named by the route. A book resume point carries an opaque location, while a music track resume point carries a time offset. A resume point of another user is reported as not found, exactly like a missing resume point, so a guessed identifier never reveals another user's position.

When the media item no longer exists, or no longer belongs to the library recorded in the resume point, the resume point is an orphan. It is deleted before the item is reported as not found. A client that reopens the item must therefore fall back to the beginning instead of assuming the stored point still exists.

When the library that recorded the resume point no longer tracks consumption progress, the stored resume point is deleted and reported as `MediaResumePointNotFound`, so turning tracking off takes effect for an existing position as well.

A kind whose catalogue does not exist yet is rejected with `UnsupportedMediaKind`, so a planned kind cannot return a resume point that start would never create. A photo is the exception among the planned kinds: it has no position representation, so it is rejected with `MediaKindCannotResume`. The invalid `None` kind is rejected with `UnsupportedMediaKind`.

#### Get Resume Point Response

```js
200 Ok
```

```json
{
  "id": "4d5e6f70-8192-4a3b-9c4d-5e6f708192a3",
  "libraryId": "5c6d7e8f-9a0b-4c1d-8e2f-3a4b5c6d7e8f",
  "mediaItemId": "7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "mediaKind": "Book",
  "lastPosition": {
    "kind": "Location",
    "timeOffsetSeconds": null,
    "location": "epubcfi(/6/4[chapter-3]!/4/2/2)"
  },
  "lastProgressFraction": 0.42,
  "lastConsumedOnUtc": "2026-10-24T12:12:00.0000000Z"
}
```

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | Guid | The Id of the resume point. |
| `libraryId` | Guid | The Id of the library that contains the item. |
| `mediaItemId` | Guid | The Id of the item to resume. |
| `mediaKind` | string | The kind of media that is resumed. |
| `lastPosition` | object | The position at which the item can be resumed. See [Position Kinds](#position-kinds). |
| `lastProgressFraction` | number | The progress fraction at which the item can be resumed, from 0 to 1. |
| `lastConsumedOnUtc` | DateTime | When the item was last consumed. |

A missing resume point returns `MediaResumePointNotFound`. A deleted item, or an item that no longer belongs to the recorded library, returns `TrackNotFound` or `BookNotFound` after the orphan resume point is removed. A kind that cannot resume returns `MediaKindCannotResume`.

### Media Kinds

`MediaKind` is shared across media contexts and is stored by its name.

| Kind | Status | Item |
| --- | --- | --- |
| `None` | Invalid | It is not a media kind and is rejected wherever a kind is required. |
| `MusicTrack` | Supported | A track of a music library. Resumes with a time offset. |
| `PodcastEpisode` | Planned | A podcast episode. Its catalogue does not exist yet. |
| `Audiobook` | Planned | An audiobook. Its catalogue does not exist yet. |
| `Movie` | Planned | A movie. Its catalogue does not exist yet. |
| `TvEpisode` | Planned | A TV episode. Its catalogue does not exist yet. |
| `Book` | Supported | A book of a book library. Resumes with an opaque location. |
| `Photo` | Planned | A photo. Its catalogue does not exist yet, and it has no position, so it cannot resume. |
| `Video` | Planned | A video. Its catalogue does not exist yet. |

Only the supported kinds can start a consumption session for now. A planned kind is rejected with `UnsupportedMediaKind`, and a `None` kind is rejected by validation. The planned kinds are listed so that clients can map the shared enumeration, not because their routes are available.

### Session Statuses

| Status | Meaning |
| --- | --- |
| `InProgress` | The session is running. Progress reports are accepted, and the record has no end time. |
| `Completed` | The session ended because completion was reported, or because a stop reached the library completion threshold. The record has an end time and the status is terminal. |
| `Stopped` | The session ended without completing. The record has an end time and the status is terminal. |

`Completed` and `Stopped` are terminal. A terminal record rejects further progress with `ConsumptionIsNotInProgress`, and further complete or stop operations with `ConsumptionIsAlreadyTerminal`. No update can move it back to `InProgress`.

### Position Kinds

A position is exactly one of two representations. The `kind` names the representation, and the fields of the other representation must be absent or null.

| Kind | Fields | Used by |
| --- | --- | --- |
| `TimeOffset` | `timeOffsetSeconds`: a non-negative finite number of seconds, within the maximum reportable range. `location` must be null or omitted. | Music tracks and the other time based kinds. |
| `Location` | `location`: a non-empty opaque string of at most 4096 characters. `timeOffsetSeconds` must be null or omitted. | Books and other page or section based kinds. |

An empty position object means no position and keeps the stored position. When a `kind` is present, exactly one representation must be present: `TimeOffset` requires `timeOffsetSeconds` and no `location`, while `Location` requires `location` and no `timeOffsetSeconds`. A position with a time offset or a location but no `kind`, a position with both representations, a position with a kind but no matching representation, an unknown or numeric kind, a negative or non-finite time offset, or an empty or too long location is rejected with `InvalidMediaPosition`. The position kind must also match the media kind: a time offset on a book, or a location on a music track, is rejected with `InvalidMediaPosition`.

A photo has no position kind, so it cannot resume. The server does not parse a book location: it stores and returns the opaque locator exactly as reported.

### Reportable Ranges

| Value | Rule |
| --- | --- |
| `progressFraction` | Finite and between 0 and 1, inclusive. |
| `consumedDurationInSeconds` | Finite, non-negative, and at most 31 days. It must also be at least the value already stored for the session. |
| `timeOffsetSeconds` | Finite, non-negative, and at most 31 days. |
| `location` | Non-empty and at most 4096 characters. |
| `consecutivePagesRead` | Non-negative. Only page based media may report a value above zero. |

The 31 day limit keeps accumulated totals representable and rejects durations that cannot describe real media. The consumed duration is the only value that must not decrease; the position and the progress fraction are latest reported values and may move backwards.

### Session Lifecycle and Resume Points

A session moves through one lifecycle:

1. [Start Consumption](#start-consumption) creates an `InProgress` record and returns its identifier.
2. [Report Consumption Progress](#report-consumption-progress) updates the latest position, progress fraction, consumed duration and consecutive page count. It never completes the session.
3. [Complete Consumption](#complete-consumption) or [Stop Consumption](#stop-consumption) ends the session, sets its end time, and makes it terminal.

A resume point is separate from the history. It is created or updated by progress and stop when the library tracks progress, the kind can resume, and a position is known. It is read by start to initialize a new session. It is deleted by completion and by a stop that reaches the completion threshold. A stopped session that did not reach the threshold keeps it for the next start. Deleting a history record does not delete the resume point, because the two are separate actions in the product.

The business key of a resume point is the user, the media kind and the media item. There is at most one resume point per key, so a progress report updates the existing row instead of inserting a second one.

Ending a session is best effort on the client side. A client can disappear between its start request and its first terminal request, for example when a full page unload happens while the start is still in flight. The record created by such a start remains `InProgress`: the API does not expire it, and start is not idempotent, so the next session creates a new record instead of closing or reusing the lost one. A lost session therefore remains in the stored history as a record that was never ended.

### Completion Threshold

Completion is explicit or threshold based, and the threshold is a per-library setting. The relevant settings are `shouldTrackConsumptionProgress` and `completionThresholdPercentage` on the library, documented in [Libraries](../library/management/Api.Libraries.md). The per-library value always wins. When a library is created without explicit values, the defaults by library type are:

| Library category | Track progress | Completion threshold |
| --- | --- | --- |
| Music audio (`Music`, `Remix`, `SoundEffect`, `CoverSongAudio`, `LiveRecordingAudio`, `InterviewAudio`) | no | none |
| Photo | no | none |
| Written content (`Book`, `EBook`, `ComicBook`, `Magazine`, `Newspaper`, `Manga`, `GraphicNovel`, `AcademicPaper`, `SheetMusic`) | yes | 100 |
| Video (`TvShow`, `Movie`, `Documentary`, `Anime`, `ConcertVideo`, `TutorialVideo`, `HomeVideo`, `YouTubeVideo`, `MusicVideo`, `LiveRecordingVideo`, `InterviewVideo`, `CoverSongVideo`) | yes | 90 |
| Audiobook and podcast (`Audiobook`, `PodcastAudio`, `PodcastVideo`) | yes | 95 |
| Other, non-media or unknown (`Playlist`, `Collection`, `Subtitles`, `Lyrics`, `Other`, any type added later) | no | none |

A stop reaches the threshold when the library tracks progress, a threshold is set, and the final progress fraction is at or above the threshold divided by 100. The comparison is done on the fraction, so a fraction exactly at the threshold completes the session. A library with tracking on and no threshold completes only when completion is reported explicitly. Tracking off never completes a session implicitly.

### Scoping and Authorization

Every route acts on the current user, read from the authenticated principal, never from the route, the query or the body. A client cannot read, progress, complete or stop another user's session through these routes.

Starting a session also applies the library ownership policy for the route library. When the current user cannot access the route library, the operation returns `NotAuthorized` before it looks up the item. When the route library is accessible, an item that does not exist and an item that belongs to another library are reported with the same kind specific not found error, so the response cannot be used to find out whether an item exists in another library. The kind specific errors are:

| Kind | Not found error |
| --- | --- |
| `MusicTrack` | `TrackNotFound` |
| `Book` | `BookNotFound` |

A missing library returns `LibraryNotFound`. The current user must have access to the library; otherwise the start returns `NotAuthorized`.

Reading a single record or a single resume point is owner only. A record or resume point of another user is reported as not found, not as forbidden, so the response shapes are indistinguishable. Admins do not write another user's session.

### Private Responses

Every response produced by these endpoints is private to the authenticated user. Each endpoint sets:

- `Cache-Control: private, no-store`
- `Vary: Authorization`

The headers are set before the operation runs, so they are present on the endpoint's problem responses too, including `403`, `404`, `409` and `422`. A request rejected by the authentication middleware with `401` before the endpoint runs is produced by the framework rather than by the endpoint. The CORS middleware may also add `Vary: Origin`; the endpoints append `Authorization` instead of replacing the header, so both values are preserved.

### Errors

Errors are returned as problem details. A request without a valid token is rejected with `401 Unauthorized` before it reaches the operation.

| Status | Error | When |
| --- | --- | --- |
| `401` | | The request carries no token, or an invalid or expired one. The body is a problem whose `title` is `Unauthorized` and whose `detail` says why. |
| `403` | `NotAuthorized` | The authenticated principal carries no user identity, or the current user cannot access the route library on start. |
| `403` | `ConsumedDurationCannotDecrease` | A progress, complete or stop request reports a consumed duration lower than the duration already stored for the session. |
| `403` | `ConsecutivePagesReadIsOnlySupportedForPageBasedMedia` | A progress report sends consecutive pages above zero for a kind that is not page based, such as a music track. |
| `403` | `MediaKindCannotResume` | A resume point is requested for a kind that cannot resume, such as a photo. |
| `404` | `LibraryNotFound` | The start route names a library that does not exist. |
| `404` | `TrackNotFound` | The start route names a music track that does not exist, or that does not belong to the route library, or a resume point names a track that no longer exists or no longer belongs to its recorded library. |
| `404` | `BookNotFound` | The start route names a book that does not exist, or that does not belong to the route library, or a resume point names a book that no longer exists or no longer belongs to its recorded library. |
| `404` | `ConsumptionRecordNotFound` | The record does not exist, or it belongs to another user. |
| `404` | `MediaResumePointNotFound` | The resume point does not exist, belongs to another user, or belongs to a library that no longer tracks consumption progress. |
| `409` | `ConsumptionIsNotInProgress` | A progress report targets a session that is already completed or stopped. |
| `409` | `ConsumptionIsAlreadyTerminal` | A complete or stop request targets a session that is already completed or stopped. |
| `409` | `ConcurrencyConflict` | Another request changed the same row after this request read it, or the storage reported a busy or locked database. Repeating the request is safe when it reports the current values. |
| `422` | see below | The request failed validation. |

The `404` and `409` problems carry the generic error code in `title` and the specific name in `detail`:

```json
{
  "title": "General.NotFound",
  "status": 404,
  "detail": "ConsumptionRecordNotFound",
  "instance": "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

A request that fails validation is rejected with `422 Unprocessable Entity` and a validation problem whose `errors` object lists the names of the validation errors under the generic `General.Validation` code:

```json
{
  "title": "General.Validation",
  "status": 422,
  "detail": "OneOrMoreValidationErrorsOccurred",
  "instance": "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/progress",
  "errors": {
    "General.Validation": [
      "InvalidProgressFraction",
      "InvalidMediaPosition"
    ]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

The validation errors of each operation are:

| Error | Operation | When |
| --- | --- | --- |
| `LibraryIdCannotBeEmpty` | Start | The library Id is missing or is not a non-empty Guid. |
| `MediaItemIdCannotBeEmpty` | Start, Get Resume Point | The item Id is missing or is not a non-empty Guid. |
| `UnsupportedMediaKind` | Start, Get Resume Point | The kind is missing, is not a member name of the media kind enumeration, or is a planned kind whose catalogue does not exist yet. A numeric value is rejected even when it is a defined enumeration value. On Get Resume Point, a photo is rejected with the 403 `MediaKindCannotResume` instead. |
| `ConsumptionRecordIdCannotBeEmpty` | Get Record, Progress, Complete, Stop | The record Id is missing or is not a non-empty Guid. |
| `InvalidMediaPosition` | Progress, Complete, Stop | The position is partially specified, uses both or neither representation for its kind, has an unknown or numeric kind, a negative or non-finite time offset, an empty or too long location, or a position kind that does not match the media kind. |
| `InvalidProgressFraction` | Progress, Complete, Stop | The progress fraction is missing, is not finite, or is outside 0 to 1. |
| `ConsumedDurationMustBeNonNegative` | Progress, Complete, Stop | The consumed duration is missing, is not finite, or is negative. |
| `ConsumedDurationIsOutOfRange` | Progress, Complete, Stop | The consumed duration is above 31 days. |
| `ConsecutivePagesReadMustBeNonNegative` | Progress | The consecutive page count is negative. |

A value that cannot be bound, such as a non-numeric progress fraction or a non-integer page count, is not reported by these validators. The endpoint does not catch binding failures, so the framework answers the request with its usual binding problem, and the web client never sends such values because its proxy endpoints rebuild the body from typed values.

### Concurrency

Every consumption record and resume point carries an optimistic concurrency version. A progress, complete or stop request reads the stored version, and the update is applied only when the stored version still matches. If another request ended or changed the session between the read and the write, the update affects no row and the request is reported as `409 ConcurrencyConflict`. A late progress report therefore never reverts a completed or stopped session, never clears its end time, and never lowers its consumed duration. A busy or locked SQLite write is reported the same way.

Repeating a progress, complete or stop request after a concurrency conflict is safe only when the client re-reads the record and reports values that are still valid for it. A complete or stop of a terminal session returns `ConsumptionIsAlreadyTerminal`, and a progress report of a terminal session returns `ConsumptionIsNotInProgress`.
