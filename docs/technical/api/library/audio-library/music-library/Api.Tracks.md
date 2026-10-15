# Lumina API

- [Lumina API](#lumina-api)
  - [Track](#track)
    - [Add Track](#add-track)
      - [Add Track Request](#add-track-request)
      - [Add Track Response](#add-track-response)
    - [Get Track](#get-track)
      - [Get Track Request](#get-track-request)
      - [Get Track Response](#get-track-response)
    - [Stream Track](#stream-track)
      - [Stream Track Request](#stream-track-request)
      - [Stream Track Response](#stream-track-response)
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
  "directPlay": true,
  "deliveryContainer": null,
  "deliveryCodec": null,
  "deliveryShape": null,
  "segmentDurationSeconds": null,
  "seekable": true,
  "streamPath": "/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream"
}
```

When the player can play the stored audio as it is, `directPlay` is `true`, and `deliveryContainer`, `deliveryCodec`, `deliveryShape` and `segmentDurationSeconds` are `null`, because the delivery is the stored file itself. When it cannot, `directPlay` is `false`, and those fields name the container, the codec, the addressing the server produces instead, and the length in seconds of each of its time addressed segments. A normalized delivery is always addressed as `TimeAddressedSegments`, and `segmentDurationSeconds` is the length of each of its segments, so that the server that produces the segments and the player that consumes them agree on it without duplicating the value on both sides. `seekable` reports whether the delivered audio can be seeked by the player. `streamPath` is relative to the API, without a host or a scheme, so that a client reaches the stream through its own application rather than the API server directly.


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
