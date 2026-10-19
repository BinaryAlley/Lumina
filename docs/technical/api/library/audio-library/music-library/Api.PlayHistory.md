# Lumina API

- [Lumina API](#lumina-api)
  - [Play History](#play-history)
    - [Start Play](#start-play)
      - [Start Play Request](#start-play-request)
      - [Start Play Response](#start-play-response)
    - [Get Play Record](#get-play-record)
      - [Get Play Record Request](#get-play-record-request)
      - [Get Play Record Response](#get-play-record-response)
    - [Report Play Progress](#report-play-progress)
      - [Report Play Progress Request](#report-play-progress-request)
      - [Report Play Progress Response](#report-play-progress-response)
    - [Complete Play](#complete-play)
      - [Complete Play Request](#complete-play-request)
      - [Complete Play Response](#complete-play-response)
    - [Stop Play](#stop-play)
      - [Stop Play Request](#stop-play-request)
      - [Stop Play Response](#stop-play-response)
    - [Concurrent Reports](#concurrent-reports)

## Play History

The play history records what the player played, how far it reached, how long the user listened, and whether the play completed or stopped. It is not a playback session: it stores no queue, current index, repeat or shuffle state, and it never restores playback. A play record belongs to the user that started it, and only that user can read it or report its progress, completion or stop.

### Start Play

#### Start Play Request

```js
POST api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/plays
Authorization: Bearer <token>
```

Records the start of a play of the track identified by the route. The library, artist, album and track Ids are taken from the route, so a request cannot substitute them with values from a body. The album and track are checked against the route scope, and the library ownership policy is evaluated, so a play is only recorded for a track of a library the current user can access. The request has no body.

#### Start Play Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "trackId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "status": "InProgress",
  "lastPositionInSeconds": 0,
  "listenedDurationInSeconds": 0,
  "startedOnUtc": "2026-10-19T12:00:00.0000000Z",
  "endedOnUtc": null,
  "updatedOnUtc": null
}
```

The returned `id` identifies the play record in the progress, complete, stop and read routes. A track that does not exist, or that does not belong to the route's library, artist and album, is reported as not found with `AlbumNotFound` or `TrackNotFound`.

### Get Play Record

#### Get Play Record Request

```js
GET api/v1/users/me/music-play-history/plays/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f
Authorization: Bearer <token>
```

Returns the play record identified by the route, when it belongs to the current user. A record of another user is reported as not found, exactly like a record that does not exist, so a guessed identifier never reveals the existence of another user's play.

#### Get Play Record Response

```js
200 Ok
```

```json
{
  "id": "2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
  "trackId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "status": "Stopped",
  "lastPositionInSeconds": 92.5,
  "listenedDurationInSeconds": 92.5,
  "startedOnUtc": "2026-10-19T12:00:00.0000000Z",
  "endedOnUtc": "2026-10-19T12:01:32.5000000Z",
  "updatedOnUtc": "2026-10-19T12:01:32.5000000Z"
}
```

The response is private to the authenticated user: it carries `Cache-Control: private, no-store` and `Vary: Authorization`.

### Report Play Progress

#### Report Play Progress Request

```js
PUT api/v1/users/me/music-play-history/plays/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/progress
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "positionInSeconds": 92.5,
  "listenedDurationInSeconds": 92.5
}
```

Reports the last position and the listened duration of an in-progress play of the current user. The listened duration can never decrease. A record of another user, or a record that does not exist, is reported as not found.

#### Report Play Progress Response

```js
200 Ok
```

The response body is an empty JSON object.

A play that is no longer in progress is reported as a conflict (`409`) with `PlayIsNotInProgress`. A report that read the play before another report completed or stopped it is also reported as a conflict (`409`), with `ConcurrencyConflict`, instead of reverting the stored state.

### Complete Play

#### Complete Play Request

```js
PUT api/v1/users/me/music-play-history/plays/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/complete
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "positionInSeconds": 354,
  "listenedDurationInSeconds": 354
}
```

Marks an in-progress play of the current user as completed, with the last position and the listened duration of the track.

#### Complete Play Response

```js
200 Ok
```

The response body is an empty JSON object. A play that is no longer in progress, or that another report changed after this one read it, is reported as a conflict (`409`) with `PlayIsNotInProgress` or `ConcurrencyConflict`.

### Stop Play

#### Stop Play Request

```js
PUT api/v1/users/me/music-play-history/plays/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "positionInSeconds": 92.5,
  "listenedDurationInSeconds": 92.5
}
```

Marks an in-progress play of the current user as stopped before the track completed, with the last position and the listened duration the user reached.

#### Stop Play Response

```js
200 Ok
```

The response body is an empty JSON object. A play that is no longer in progress, or that another report changed after this one read it, is reported as a conflict (`409`) with `PlayIsNotInProgress` or `ConcurrencyConflict`.

### Concurrent Reports

Every play record carries an optimistic concurrency version. A progress, complete or stop report reads the stored version, and the update is applied only when the stored version still matches. If another report ended or changed the play between the read and the write, the update affects no row and the request is reported as `409 ConcurrencyConflict`. A late progress report therefore never reverts a completed or stopped play, never clears its end time, and never lowers its listened duration. A busy or locked SQLite write is reported the same way.
