# Lumina API

- [Lumina API](#lumina-api)
  - [Track](#track)
    - [Add Track](#add-track)
      - [Add Track Request](#add-track-request)
      - [Add Track Response](#add-track-response)
    - [Get Track](#get-track)
      - [Get Track Request](#get-track-request)
      - [Get Track Response](#get-track-response)
    - [Shuffle Tracks](#shuffle-tracks)
      - [Shuffle Tracks Request](#shuffle-tracks-request)
      - [Shuffle Tracks Response](#shuffle-tracks-response)
    - [Stream Track](#stream-track)
      - [Stream Track Request](#stream-track-request)
      - [Stream Track Response](#stream-track-response)
    - [Stream Track Segment](#stream-track-segment)
      - [Stream Track Segment Request](#stream-track-segment-request)
      - [Stream Track Segment Response](#stream-track-segment-response)
    - [Get Playback Manifest](#get-playback-manifest)
      - [Get Playback Manifest Request](#get-playback-manifest-request)
      - [Get Playback Manifest Response](#get-playback-manifest-response)
    - [Update Track](#update-track)
      - [Update Track Request](#update-track-request)
      - [Update Track Response](#update-track-response)
    - [Delete Track](#delete-track)

## Track

### Add Track

#### Add Track Request

```js
POST api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks
```

```json
{
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "disambiguation": "album version",
    "releaseInfo": {
      "originalReleaseDate": "1975-10-31",
      "originalReleaseYear": 1975,
      "reReleaseDate": "2011-11-21",
      "reReleaseYear": 2011,
      "releaseCountry": "GB",
      "releaseVersion": "Original"
    },
    "language": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "originalLanguage": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "tags": [
      { "name": "classic" },
      { "name": "epic" }
    ],
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "isVideo": false,
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "acoustId": "f0e1d2c3-b4a5-4c6d-8e7f-9a0b1c2d3e4f",
    "replayGainTrackGain": -7.5,
    "replayGainTrackPeak": 0.9877,
    "replayGainAlbumGain": -6.8,
    "replayGainAlbumPeak": 0.9999
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": {
    "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
    "title": "Bohemian Rhapsody",
    "type": "Song",
    "languages": [
      { "languageCode": "en", "languageName": "English", "nativeName": "English" }
    ],
    "iswcs": ["T-010489707-6"]
  },
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "moods": [
    { "name": "dramatic" },
    { "name": "anxious" }
  ],
  "isrcs": [
    { "value": "GBUM71029604" }
  ],
  "contributors": [
    { "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d", "role": "Vocals" },
    { "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e", "role": "Guitar" }
  ],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    },
    {
      "value": 4.8,
      "maxValue": 5,
      "source": "LastFm",
      "voteCount": 1234
    }
  ]
}
```

#### Add Track Response

```js
201 Created
```

```json
{
  "id": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "container": "FLAC",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "disambiguation": "album version",
    "releaseInfo": { "originalReleaseDate": "1975-10-31", "originalReleaseYear": 1975, "reReleaseDate": "2011-11-21", "reReleaseYear": 2011, "releaseCountry": "GB", "releaseVersion": "Original" },
    "language": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
    "originalLanguage": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
    "tags": [ { "name": "classic" }, { "name": "epic" } ],
    "genres": [ { "name": "Rock" }, { "name": "Progressive Rock" } ],
    "isVideo": false,
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "acoustId": "f0e1d2c3-b4a5-4c6d-8e7f-9a0b1c2d3e4f",
    "replayGainTrackGain": -7.5,
    "replayGainTrackPeak": 0.9877,
    "replayGainAlbumGain": -6.8,
    "replayGainAlbumPeak": 0.9999
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": {
    "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
    "title": "Bohemian Rhapsody",
    "type": "Song",
    "languages": [
      { "languageCode": "en", "languageName": "English", "nativeName": "English" }
    ],
    "iswcs": ["T-010489707-6"]
  },
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": "2025-01-20T14:45:00.0000000Z",
  "moods": [
    {
      "name": "dramatic"
    },
    {
      "name": "anxious"
    }
  ],
  "isrcs": [
    {
      "value": "GBUM71029604"
    }
  ],
  "contributors": [
    {
      "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d",
      "role": "Vocals"
    },
    {
      "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e",
      "role": "Guitar"
    }
  ],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    },
    {
      "value": 4.8,
      "maxValue": 5,
      "source": "LastFm",
      "voteCount": 1234
    }
  ]
}
```


### Get Track

#### Get Track Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
```

#### Get Track Response

```js
200 Ok
```

```json
{
  "id": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "container": "FLAC",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "disambiguation": "album version",
    "releaseInfo": { "originalReleaseDate": "1975-10-31", "originalReleaseYear": 1975, "reReleaseDate": "2011-11-21", "reReleaseYear": 2011, "releaseCountry": "GB", "releaseVersion": "Original" },
    "language": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
    "originalLanguage": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
    "tags": [ { "name": "classic" }, { "name": "epic" } ],
    "genres": [ { "name": "Rock" }, { "name": "Progressive Rock" } ],
    "isVideo": false,
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "acoustId": "f0e1d2c3-b4a5-4c6d-8e7f-9a0b1c2d3e4f",
    "replayGainTrackGain": -7.5,
    "replayGainTrackPeak": 0.9877,
    "replayGainAlbumGain": -6.8,
    "replayGainAlbumPeak": 0.9999
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": {
    "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
    "title": "Bohemian Rhapsody",
    "type": "Song",
    "languages": [
      { "languageCode": "en", "languageName": "English", "nativeName": "English" }
    ],
    "iswcs": ["T-010489707-6"]
  },
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": "2025-01-20T14:45:00.0000000Z",
  "moods": [
    {
      "name": "dramatic"
    },
    {
      "name": "anxious"
    }
  ],
  "isrcs": [
    {
      "value": "GBUM71029604"
    }
  ],
  "contributors": [
    {
      "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d",
      "role": "Vocals"
    },
    {
      "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e",
      "role": "Guitar"
    }
  ],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    },
    {
      "value": 4.8,
      "maxValue": 5,
      "source": "LastFm",
      "voteCount": 1234
    }
  ]
}
```


### Shuffle Tracks

#### Shuffle Tracks Request

```js
POST api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/tracks/shuffle
```

```json
{
  "count": 100,
  "artistId": null,
  "albumId": null,
  "excludedTrackIds": [
    "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"
  ]
}
```

Returns a uniformly random selection of tracks from every track of the library, optionally scoped to one artist or one album, and excludes the tracks in `excludedTrackIds`. The database picks the rows at random, so the result is not limited to the albums or artists that happen to be stored or loaded first. `count` must be between 1 and 500. When `artistId` is present, only tracks whose album belongs to that artist are sampled; when `albumId` is present, only tracks of that album are sampled. The tracks are returned to an Admin, who can shuffle all libraries, or to the owner of the library.

#### Shuffle Tracks Response

```js
200 Ok
```

```json
[
  {
    "trackId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
    "title": "Bohemian Rhapsody",
    "durationInSeconds": 354,
    "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
    "albumTitle": "A Night at the Opera",
    "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
    "coverPath": "\\media\\music\\music-3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a\\QUEEN\\Album\\A night at the opera-e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f\\cover.jpg"
  }
]
```

The response holds at most `count` tracks, and fewer when the scope has fewer tracks that are not excluded. A repeated request returns a different random selection and a different order.


### Stream Track

#### Stream Track Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream
Authorization: Bearer <token>
Range: bytes=0-1048575
```

Streams the audio file of the track identified by the route, with HTTP range support. The `Range` header is optional: without it the whole file is streamed, and with it only the requested range is streamed, so that a player can start playback before the whole file is downloaded and can seek within the file. The audio file is streamed to an Admin, who can stream the tracks of all libraries, or to the owner of the library of the track.

#### Stream Track Response

```js
200 Ok
```

or, when a range is requested:

```js
206 Partial Content
Content-Range: bytes 0-1048575/3542771
Accept-Ranges: bytes
```

The response is the raw bytes of the audio file, served with the media type of the file when it is an audio type, or `application/octet-stream` otherwise. Every response carries `X-Content-Type-Options: nosniff`, `Cache-Control: private` and `Vary: Authorization`, together with a `Last-Modified` and an `ETag` derived from the size and the modification time of the file.


### Stream Track Segment

#### Stream Track Segment Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0
Authorization: Bearer <token>
```

Streams one segment of the normalized audio of the track identified by the route. A track the player cannot play as it is stored is delivered as time addressed segments instead of bytes: the server produces the whole track into a temporary cache with a target segment duration of `segmentDurationSeconds` seconds, cutting at the first packet at or after each target boundary, and serves each produced segment as an independent container file. A real segment start can therefore differ from an exact multiple of the target duration, and a consumer must use the start the response reports rather than assume the grid. The segment index is zero based.

The `deliveryId` is the opaque handle the playback manifest reported in `deliveryId`. It is bound to the user the manifest was issued to and to the track it describes, and it captures the delivery decision, the quality cap, the source file version and the library content locations the manifest declared. The segment route resolves that handle instead of recomputing the delivery from a capability list, so the segments always belong to exactly the delivery the manifest declared. Library ownership is re-evaluated on every segment request, so revoked access stops playback immediately; the track membership and the content locations are the ones captured at manifest time, so a deleted track row whose file remains, or a removed content location, stays streamable until the handle expires, within 12 hours. A handle that is unknown, expired, or was issued to another user or track is reported as not found. A track the manifest declared as a direct play has no delivery handle at all, so a segment request for it is also reported as not found with `PlaybackDeliveryNotFound`, never as the end of the audio. A source file that changed since the manifest declared the delivery is reported as a conflict instead of starting a second production. The handle is exactly 32 lowercase hexadecimal characters, and a value that does not match that format is rejected rather than normalized into another handle.

A byte range cannot be mapped to a time in this delivery, so a player seeks by choosing the segment that covers the target time. A segment is a small independent container file that is already fetched as a whole, so the player reads it from its first packet and discards the packets that end before the target; it never seeks inside the container itself, because a container that numbers its frames across the whole track would then add that number to the segment offset twice. The playback manifest reports the root relative path of this route in `segmentStreamPathTemplate`, with `{deliveryId}` and `{segmentIndex}` placeholders, and the player expands both from the manifest. A segment is served to an Admin, who can stream the tracks of all libraries, or to the owner of the library of the track.

#### Stream Track Segment Response

```js
200 Ok
Content-Type: audio/aac
Content-Length: 240128
```

The response is the raw bytes of one produced segment, served with the media type of the delivered container when the server can name it, or `application/octet-stream` otherwise. Every response carries `X-Content-Type-Options: nosniff`, `Cache-Control: private, no-store`, `Vary: Authorization`, `X-Lumina-Segment-Count`, the number of segments the production holds, and `X-Segment-Start-Seconds`, the exact start of the segment on the track timeline, in seconds. The count tells a player where the audio ends without probing for the end with a request that returns not found, and the start lets it offset the packets of a container that numbers its frames from the start of the track, instead of assuming that a segment starts at an exact multiple of its duration. Range requests are not used: the segment is the addressing unit, and the next segment is a separate request. The first request for a track can take longer than the following ones, because the whole normalized audio is produced and cached before its first segment is published. A segment index that the production does not hold is reported as not found with the `NormalizedTrackSegmentNotFound` detail, which is the only not-found answer that means the end of the audio. A track, library or audio file that disappeared, a delivery handle that is unknown, expired, or was issued to another user or track, and a cache entry that was evicted, are reported with their own not-found details instead, and a request that arrives while every normalization job slot is in use is reported as unavailable (`503`), because it is a transient condition that a player retries. A delivery handle whose captured shape is not time addressed segments is a conflict (`409`), because the segment route cannot index it.


### Get Playback Manifest

#### Get Playback Manifest Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/playback-manifest?capabilities=mp3/mp3,flac/flac,ogg/opus,ogg/vorbis,wav/pcm,mp4/aac
Authorization: Bearer <token>
```

Returns the playback manifest of the track identified by the route: the format of its stored audio, the delivery the server decided on for the requesting player, and the path of its stream, so that a player is told what to fetch and how before it fetches any audio. The `capabilities` query parameter is optional, and its value is a comma separated list of the `container/codec` pairs the player advertises it can demux and decode directly, matched case insensitively, with at most 32 pairs read from one request. When the parameter is absent, empty, or lists no valid pair, the default capabilities of the bundled player apply. The manifest is returned to an Admin, who can build the manifest of the tracks of all libraries, or to the owner of the library of the track.

#### Get Playback Manifest Response

```js
200 Ok
```

```json
{
  "trackId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "durationInSeconds": 354,
  "sourceContainer": "FLAC",
  "sourceCodec": "FLAC",
  "sampleRate": 44100,
  "channels": 2,
  "bitDepth": 16,
  "replayGainTrackGain": -7.5,
  "replayGainTrackPeak": 0.9877,
  "replayGainAlbumGain": -6.8,
  "replayGainAlbumPeak": 0.9999,
  "isDirectPlay": true,
  "deliveryContainer": null,
  "deliveryCodec": null,
  "deliveryShape": null,
  "segmentDurationSeconds": null,
  "isSeekable": true,
  "streamPath": "/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream",
  "deliveryId": null,
  "segmentStreamPathTemplate": null
}
```

When the player can play the stored audio as it is, `isDirectPlay` is `true`, and `deliveryContainer`, `deliveryCodec`, `deliveryShape`, `segmentDurationSeconds`, `deliveryId` and `segmentStreamPathTemplate` are `null`, because the delivery is the stored file itself, addressed and seeked by byte range. When it cannot, `isDirectPlay` is `false`, and those fields name the container, the codec, the addressing the server produces instead, the length in seconds of each of its time addressed segments, the opaque handle of the delivery, and the root relative path template of the segment route, with `{deliveryId}` and `{segmentIndex}` placeholders. `durationInSeconds` is the probed duration; a normalized production can be slightly longer or shorter, and its exact produced duration is reported by the `X-Lumina-Track-Duration-Seconds` header of its segments. A normalized delivery is always addressed as `TimeAddressedSegments`, and `segmentDurationSeconds` is the nominal length of each of its segments, so that the server that produces the segments and the player that consumes them agree on the target without duplicating the value on both sides. The `deliveryId` binds the delivery to the user and the track, and it captures the quality cap and the source file version that were in effect when the manifest was read, so a segment request addresses exactly that delivery instead of recomputing it. A player that cannot play a track directly fetches each segment from that template in index order and seeks by segment index, never by byte range. `isSeekable` reports whether the delivered audio can be seeked by the player. `streamPath` is relative to the API, without a host or a scheme, so that a client reaches the stream through its own application rather than the API server directly.

For example, a delivery of a track the player cannot decode is declared as follows:

```json
{
  "trackId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "isDirectPlay": false,
  "deliveryContainer": "AAC",
  "deliveryCodec": "AAC",
  "deliveryShape": "TimeAddressedSegments",
  "segmentDurationSeconds": 6,
  "isSeekable": true,
  "streamPath": "/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream",
  "deliveryId": "8f14e45fceea167a5a36dedd4bea2543",
  "segmentStreamPathTemplate": "/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream/deliveries/{deliveryId}/segments/{segmentIndex}"
}
```


### Update Track

#### Update Track Request

```js
PUT api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
```

```json
{
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "disambiguation": "album version",
    "releaseInfo": {
      "originalReleaseDate": "1975-10-31",
      "originalReleaseYear": 1975,
      "reReleaseDate": "2011-11-21",
      "reReleaseYear": 2011,
      "releaseCountry": "GB",
      "releaseVersion": "Original"
    },
    "language": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "originalLanguage": {
      "languageCode": "en",
      "languageName": "English",
      "nativeName": "English"
    },
    "tags": [
      { "name": "classic" },
      { "name": "epic" }
    ],
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "isVideo": false,
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "acoustId": "f0e1d2c3-b4a5-4c6d-8e7f-9a0b1c2d3e4f",
    "replayGainTrackGain": -7.5,
    "replayGainTrackPeak": 0.9877,
    "replayGainAlbumGain": -6.8,
    "replayGainAlbumPeak": 0.9999
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": {
    "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
    "title": "Bohemian Rhapsody",
    "type": "Song",
    "languages": [
      { "languageCode": "en", "languageName": "English", "nativeName": "English" }
    ],
    "iswcs": ["T-010489707-6"]
  },
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "moods": [
    { "name": "dramatic" },
    { "name": "anxious" }
  ],
  "isrcs": [
    { "value": "GBUM71029604" }
  ],
  "contributors": [
    { "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d", "role": "Vocals" },
    { "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e", "role": "Guitar" }
  ],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    },
    {
      "value": 4.8,
      "maxValue": 5,
      "source": "LastFm",
      "voteCount": 1234
    }
  ]
}
```

#### Update Track Response

```js
200 Ok
```

```json
{
  "id": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "container": "FLAC",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "disambiguation": "album version",
    "releaseInfo": { "originalReleaseDate": "1975-10-31", "originalReleaseYear": 1975, "reReleaseDate": "2011-11-21", "reReleaseYear": 2011, "releaseCountry": "GB", "releaseVersion": "Original" },
    "language": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
    "originalLanguage": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
    "tags": [ { "name": "classic" }, { "name": "epic" } ],
    "genres": [ { "name": "Rock" }, { "name": "Progressive Rock" } ],
    "isVideo": false,
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "acoustId": "f0e1d2c3-b4a5-4c6d-8e7f-9a0b1c2d3e4f",
    "replayGainTrackGain": -7.5,
    "replayGainTrackPeak": 0.9877,
    "replayGainAlbumGain": -6.8,
    "replayGainAlbumPeak": 0.9999
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": {
    "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
    "title": "Bohemian Rhapsody",
    "type": "Song",
    "languages": [
      { "languageCode": "en", "languageName": "English", "nativeName": "English" }
    ],
    "iswcs": ["T-010489707-6"]
  },
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": "2025-01-20T14:45:00.0000000Z",
  "moods": [
    {
      "name": "dramatic"
    },
    {
      "name": "anxious"
    }
  ],
  "isrcs": [
    {
      "value": "GBUM71029604"
    }
  ],
  "contributors": [
    {
      "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d",
      "role": "Vocals"
    },
    {
      "contributorId": "7b4e1d3f-2c5a-4e6b-9d7e-0f1a2b3c4d5e",
      "role": "Guitar"
    }
  ],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    },
    {
      "value": 4.8,
      "maxValue": 5,
      "source": "LastFm",
      "voteCount": 1234
    }
  ]
}
```


### Delete Track

```js
DELETE api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/{trackId}
```

```js
200 Ok
```

Deletes the track identified by the route.
