# Lumina API

- [Lumina API](#lumina-api)
  - [Track](#track)
    - [Get Album Tracks](#get-album-tracks)
      - [Get Album Tracks Request](#get-album-tracks-request)
      - [Get Album Tracks Response](#get-album-tracks-response)
    - [Get Album Tracks Lite](#get-album-tracks-lite)
      - [Get Album Tracks Lite Request](#get-album-tracks-lite-request)
      - [Get Album Tracks Lite Response](#get-album-tracks-lite-response)
    - [Add Track](#add-track)
      - [Add Track Request](#add-track-request)
      - [Add Track Response](#add-track-response)
    - [Get Track](#get-track)
      - [Get Track Request](#get-track-request)
      - [Get Track Response](#get-track-response)
    - [Update Track](#update-track)
      - [Update Track Request](#update-track-request)
      - [Update Track Response](#update-track-response)
    - [Delete Track](#delete-track)

## Track

### Get Album Tracks

#### Get Album Tracks Request

```js
GET api/v1/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks
```

#### Get Album Tracks Response

```js
200 Ok
```

```json
[
  {
    "id": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
    "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
    "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
    "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "trackNumber": 1,
    "discNumber": 1,
    "script": "Latn",
    "key": "CMajor",
    "bpm": 72,
    "work": "Bohemian Rhapsody",
    "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
    "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
    "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
    "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
    "updatedOnUtc": null,
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
    ],
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "originalReleaseDate": "1975-10-31",
    "originalReleaseYear": 1975,
    "reReleaseDate": null,
    "reReleaseYear": null,
    "releaseCountry": "GB",
    "releaseVersion": "Original",
    "languageCode": "en",
    "languageName": "English",
    "languageNativeName": "English",
    "originalLanguageCode": "en",
    "originalLanguageName": "English",
    "originalLanguageNativeName": "English",
    "genres": [
      {
        "name": "Rock"
      },
      {
        "name": "Progressive Rock"
      }
    ],
    "tags": [
      {
        "name": "classic"
      },
      {
        "name": "epic"
      }
    ],
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
    ]
  }
]
```


### Get Album Tracks Lite

#### Get Album Tracks Lite Request

```js
GET api/v1/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/lite?currentPage=1&perPage=10
```

| Query Parameter | Type | Description |
| --- | --- | --- |
| `currentPage` | `int` | Optional. The page of results to retrieve. |
| `perPage` | `int` | Optional. The maximum number of tracks to retrieve per page. |

#### Get Album Tracks Lite Response

```js
200 Ok
```

```json
{
  "data": [
    {
      "id": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
      "title": "Bohemian Rhapsody",
      "trackNumber": 1,
      "discNumber": 1
    }
  ],
  "currentPage": 1,
  "perPage": 10,
  "count": 1,
  "numberOfPages": 1
}
```


### Add Track

#### Add Track Request

```js
POST api/v1/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks
```

```json
{
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "releaseInfo": {
      "originalReleaseDate": "1975-10-31",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
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
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "tags": [
      { "name": "classic" },
      { "name": "epic" }
    ]
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": "Bohemian Rhapsody",
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
  "credits": [
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
  ],
  "moods": [
    { "name": "dramatic" },
    { "name": "anxious" }
  ],
  "isrcs": [
    { "value": "GBUM71029604" }
  ]
}
```

#### Add Track Response

```js
201 Created
```

The response carries the full details of the newly created track, using the same shape as the [Get Track Response](#get-track-response).


### Get Track

#### Get Track Request

```js
GET api/v1/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
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
  "title": "Bohemian Rhapsody",
  "originalTitle": "Bohemian Rhapsody",
  "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": "Bohemian Rhapsody",
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null,
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
  ],
  "durationInSeconds": 354,
  "sampleRate": 44100,
  "channels": 2,
  "bitDepth": 16,
  "audioCodec": "FLAC",
  "bitrate": 980,
  "originalReleaseDate": "1975-10-31",
  "originalReleaseYear": 1975,
  "reReleaseDate": null,
  "reReleaseYear": null,
  "releaseCountry": "GB",
  "releaseVersion": "Original",
  "languageCode": "en",
  "languageName": "English",
  "languageNativeName": "English",
  "originalLanguageCode": "en",
  "originalLanguageName": "English",
  "originalLanguageNativeName": "English",
  "genres": [
    {
      "name": "Rock"
    },
    {
      "name": "Progressive Rock"
    }
  ],
  "tags": [
    {
      "name": "classic"
    },
    {
      "name": "epic"
    }
  ],
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
  ]
}
```


### Update Track

#### Update Track Request

```js
PUT api/v1/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
```

```json
{
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "trackId": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
  "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
  "metadata": {
    "title": "Bohemian Rhapsody",
    "originalTitle": "Bohemian Rhapsody",
    "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980,
    "releaseInfo": {
      "originalReleaseDate": "1975-10-31",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
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
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "tags": [
      { "name": "classic" },
      { "name": "epic" }
    ]
  },
  "trackNumber": 1,
  "discNumber": 1,
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "work": "Bohemian Rhapsody",
  "musicBrainzRecordingId": "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a",
  "musicBrainzTrackId": "e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b",
  "musicBrainzWorkId": "f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c",
  "credits": [
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
  ],
  "moods": [
    { "name": "dramatic" },
    { "name": "anxious" }
  ],
  "isrcs": [
    { "value": "GBUM71029604" }
  ]
}
```

#### Update Track Response

```js
200 Ok
```

The response carries the full details of the updated track, using the same shape as the [Get Track Response](#get-track-response).


### Delete Track

```js
DELETE api/v1/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f
```

```js
200 Ok
```

Deletes the track identified by the path.