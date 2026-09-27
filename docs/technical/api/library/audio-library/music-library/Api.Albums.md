# Lumina API

- [Lumina API](#lumina-api)
  - [Album](#album)
    - [Add Album](#add-album)
      - [Add Album Request](#add-album-request)
      - [Add Album Response](#add-album-response)
    - [Get Artist Albums](#get-artist-albums)
      - [Get Artist Albums Request](#get-artist-albums-request)
      - [Get Artist Albums Response](#get-artist-albums-response)
    - [Get Artist Albums Lite](#get-artist-albums-lite)
      - [Get Artist Albums Lite Request](#get-artist-albums-lite-request)
      - [Get Artist Albums Lite Response](#get-artist-albums-lite-response)
    - [Get Album](#get-album)
      - [Get Album Request](#get-album-request)
      - [Get Album Response](#get-album-response)
    - [Update Album](#update-album)
      - [Update Album Request](#update-album-request)
      - [Update Album Response](#update-album-response)
    - [Delete Album](#delete-album)
    - [Get Album Tracks](#get-album-tracks)
      - [Get Album Tracks Request](#get-album-tracks-request)
      - [Get Album Tracks Response](#get-album-tracks-response)
    - [Get Album Tracks Lite](#get-album-tracks-lite)
      - [Get Album Tracks Lite Request](#get-album-tracks-lite-request)
      - [Get Album Tracks Lite Response](#get-album-tracks-lite-response)

## Album

### Add Album

#### Add Album Request

```js
POST api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums
```

```json
{
  "metadata": {
    "title": "A Night at the Opera",
    "originalTitle": "A Night at the Opera",
    "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
    "releaseInfo": {
      "originalReleaseDate": "1975-11-21",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Remastered"
    },
    "releaseType": "Album",
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12,
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
      { "name": "vinyl" }
    ]
  },
  "mediaFormat": "CD",
  "barcode": "0042282778329",
  "catalogNumber": "EMC 4008",
  "musicBrainzReleaseId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "musicBrainzReleaseGroupId": "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e",
  "musicBrainzReleaseArtistId": "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f",
  "contributors": [
    { "contributorId": "0e7b4a6c-5f8d-6b9e-ca0b-3c4d5e6f7a8b", "role": "Producer" },
    { "contributorId": "1a8c5b7d-6a9e-7c0f-db1c-4d5e6f7a8b9c", "role": "Engineer" }
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
  "tracks": [
    {
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
      ],
      "moods": [
        { "name": "dramatic" },
        { "name": "anxious" }
      ],
      "isrcs": [
        { "value": "GBUM71029604" }
      ]
    },
    {
      "path": "/music/queen/a-night-at-the-opera/07-youre-my-best-friend.flac",
      "metadata": {
        "title": "You're My Best Friend",
        "originalTitle": "You're My Best Friend",
        "description": "A song by the British rock band Queen, written by bass guitarist John Deacon. It was originally released on the album A Night at the Opera in 1975 and as a single in 1976.",
        "durationInSeconds": 181,
        "sampleRate": 44100,
        "channels": 2,
        "bitDepth": 16,
        "audioCodec": "FLAC",
        "bitrate": 912,
        "releaseInfo": {
          "originalReleaseDate": "1976-06-18",
          "originalReleaseYear": 1976,
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
          { "name": "Pop Rock" }
        ],
        "tags": [
          { "name": "classic" },
          { "name": "love" }
        ]
      },
      "trackNumber": 7,
      "discNumber": 1,
      "script": "Latn",
      "key": "BMajor",
      "bpm": 118,
      "work": "You're My Best Friend",
      "musicBrainzRecordingId": "0f3e4d5c-6b7a-4f8e-9d0c-1b2a3c4d5e6f",
      "musicBrainzTrackId": "1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d",
      "musicBrainzWorkId": "2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e",
      "contributors": [
        { "contributorId": "6a3f0c2d-1b4e-4f5a-8c6d-9e0f1a2b3c4d", "role": "Vocals" },
        { "contributorId": "8c5f2e4a-3d6b-4f7c-ae8f-1a2b3c4d5e6f", "role": "BassGuitar" }
      ],
      "ratings": [
        {
          "value": 4.2,
          "maxValue": 5,
          "source": "MusicBrainz",
          "voteCount": 1234
        },
        {
          "value": 4.4,
          "maxValue": 5,
          "source": "LastFm",
          "voteCount": 567
        }
      ],
      "moods": [
        { "name": "happy" },
        { "name": "warm" }
      ],
      "isrcs": [
        { "value": "GBUM71029609" }
      ]
    }
  ]
}
```

#### Add Album Response

```js
201 Created
```

```json
{
  "id": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "metadata": {
    "title": "A Night at the Opera",
    "originalTitle": "A Night at the Opera",
    "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
    "releaseType": "Album",
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12,
    "releaseInfo": {
      "originalReleaseDate": "1975-11-21",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Remastered"
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
        "name": "vinyl"
      }
    ]
  },
  "mediaFormat": "CD",
  "barcode": "0042282778329",
  "catalogNumber": "EMC 4008",
  "musicBrainzReleaseId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "musicBrainzReleaseGroupId": "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e",
  "musicBrainzReleaseArtistId": "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null,
  "contributors": [
    {
      "contributorId": "0e7b4a6c-5f8d-6b9e-ca0b-3c4d5e6f7a8b",
      "role": "Producer"
    },
    {
      "contributorId": "1a8c5b7d-6a9e-7c0f-db1c-4d5e6f7a8b9c",
      "role": "Engineer"
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
  ],
  "tracks": [
    {
      "id": "9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
      "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
      "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
      "path": "/music/queen/a-night-at-the-opera/01-bohemian-rhapsody.flac",
      "metadata": {
        "title": "Bohemian Rhapsody",
        "originalTitle": "Bohemian Rhapsody",
        "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
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
        "tags": [
          { "name": "classic" },
          { "name": "epic" }
        ],
        "genres": [
          { "name": "Rock" },
          { "name": "Progressive Rock" }
        ],
        "durationInSeconds": 354,
        "sampleRate": 44100,
        "channels": 2,
        "bitDepth": 16,
        "audioCodec": "FLAC",
        "bitrate": 980
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
      "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
      "updatedOnUtc": null,
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
      ],
      "moods": [
        { "name": "dramatic" },
        { "name": "anxious" }
      ],
      "isrcs": [
        { "value": "GBUM71029604" }
      ]
    },
    {
      "id": "8e1f2a3b-4c5d-4e6f-8a9b-0c1d2e3f4a5b",
      "albumId": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
      "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
      "path": "/music/queen/a-night-at-the-opera/07-youre-my-best-friend.flac",
      "metadata": {
        "title": "You're My Best Friend",
        "originalTitle": "You're My Best Friend",
        "description": "A song by the British rock band Queen, written by bass guitarist John Deacon. It was originally released on the album A Night at the Opera in 1975 and as a single in 1976.",
        "releaseInfo": {
          "originalReleaseDate": "1976-06-18",
          "originalReleaseYear": 1976,
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
        "tags": [
          { "name": "classic" },
          { "name": "love" }
        ],
        "genres": [
          { "name": "Rock" },
          { "name": "Pop Rock" }
        ],
        "durationInSeconds": 181,
        "sampleRate": 44100,
        "channels": 2,
        "bitDepth": 16,
        "audioCodec": "FLAC",
        "bitrate": 912
      },
      "trackNumber": 7,
      "discNumber": 1,
      "script": "Latn",
      "key": "BMajor",
      "bpm": 118,
      "work": "You're My Best Friend",
      "musicBrainzRecordingId": "0f3e4d5c-6b7a-4f8e-9d0c-1b2a3c4d5e6f",
      "musicBrainzTrackId": "1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d",
      "musicBrainzWorkId": "2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e",
      "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
      "updatedOnUtc": null,
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
          "value": 4.2,
          "maxValue": 5,
          "source": "MusicBrainz",
          "voteCount": 1234
        },
        {
          "value": 4.4,
          "maxValue": 5,
          "source": "LastFm",
          "voteCount": 567
        }
      ],
      "moods": [
        { "name": "happy" },
        { "name": "warm" }
      ],
      "isrcs": [
        { "value": "GBUM71029609" }
      ]
    }
  ]
}
```


### Get Artist Albums

#### Get Artist Albums Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums
```

#### Get Artist Albums Response

```js
200 Ok
```

```json
[
  {
    "id": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
    "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
    "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
    "metadata": {
      "title": "A Night at the Opera",
      "originalTitle": "A Night at the Opera",
      "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
      "releaseType": "Album",
      "releaseStatus": "Official",
      "totalDiscs": 1,
      "totalTracks": 12,
      "releaseInfo": {
        "originalReleaseDate": "1975-11-21",
        "originalReleaseYear": 1975,
        "reReleaseDate": null,
        "reReleaseYear": null,
        "releaseCountry": "GB",
        "releaseVersion": "Remastered"
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
          "name": "vinyl"
        }
      ]
    },
    "mediaFormat": "CD",
    "barcode": "0042282778329",
    "catalogNumber": "EMC 4008",
    "musicBrainzReleaseId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
    "musicBrainzReleaseGroupId": "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e",
    "musicBrainzReleaseArtistId": "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f",
    "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
    "updatedOnUtc": null,
    "contributors": [
      {
        "contributorId": "0e7b4a6c-5f8d-6b9e-ca0b-3c4d5e6f7a8b",
        "role": "Producer"
      },
      {
        "contributorId": "1a8c5b7d-6a9e-7c0f-db1c-4d5e6f7a8b9c",
        "role": "Engineer"
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
    ],
    "tracks": []
  }
]
```


### Get Artist Albums Lite

#### Get Artist Albums Lite Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/lite?currentPage=1&perPage=10
```

| Query Parameter | Type | Description |
| --- | --- | --- |
| `currentPage` | `int` | Optional. The page of results to retrieve. |
| `perPage` | `int` | Optional. The maximum number of albums to retrieve per page. |

#### Get Artist Albums Lite Response

```js
200 Ok
```

```json
{
  "data": [
    {
      "id": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
      "title": "A Night at the Opera",
      "totalTracks": 12
    }
  ],
  "currentPage": 1,
  "perPage": 10,
  "count": 1,
  "numberOfPages": 1
}
```


### Get Album

#### Get Album Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f
```

#### Get Album Response

```js
200 Ok
```

```json
{
  "id": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "metadata": {
    "title": "A Night at the Opera",
    "originalTitle": "A Night at the Opera",
    "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
    "releaseType": "Album",
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12,
    "releaseInfo": {
      "originalReleaseDate": "1975-11-21",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Remastered"
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
        "name": "vinyl"
      }
    ]
  },
  "mediaFormat": "CD",
  "barcode": "0042282778329",
  "catalogNumber": "EMC 4008",
  "musicBrainzReleaseId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "musicBrainzReleaseGroupId": "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e",
  "musicBrainzReleaseArtistId": "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null,
  "contributors": [
    {
      "contributorId": "0e7b4a6c-5f8d-6b9e-ca0b-3c4d5e6f7a8b",
      "role": "Producer"
    },
    {
      "contributorId": "1a8c5b7d-6a9e-7c0f-db1c-4d5e6f7a8b9c",
      "role": "Engineer"
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
  ],
  "tracks": []
}
```


### Update Album

#### Update Album Request

```js
PUT api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f
```

```json
{
  "metadata": {
    "title": "A Night at the Opera",
    "originalTitle": "A Night at the Opera",
    "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
    "releaseInfo": {
      "originalReleaseDate": "1975-11-21",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Remastered"
    },
    "releaseType": "Album",
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12,
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
      { "name": "vinyl" }
    ]
  },
  "mediaFormat": "CD",
  "barcode": "0042282778329",
  "catalogNumber": "EMC 4008",
  "musicBrainzReleaseId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "musicBrainzReleaseGroupId": "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e",
  "musicBrainzReleaseArtistId": "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f",
  "contributors": [
    { "contributorId": "0e7b4a6c-5f8d-6b9e-ca0b-3c4d5e6f7a8b", "role": "Producer" },
    { "contributorId": "1a8c5b7d-6a9e-7c0f-db1c-4d5e6f7a8b9c", "role": "Engineer" }
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

#### Update Album Response

```js
200 Ok
```

```json
{
  "id": "e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f",
  "artistId": "d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a",
  "libraryId": "3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
  "metadata": {
    "title": "A Night at the Opera",
    "originalTitle": "A Night at the Opera",
    "description": "The fourth studio album by the British rock band Queen, released in 1975. It was the most expensive album ever recorded at the time of its release.",
    "releaseType": "Album",
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12,
    "releaseInfo": {
      "originalReleaseDate": "1975-11-21",
      "originalReleaseYear": 1975,
      "reReleaseDate": null,
      "reReleaseYear": null,
      "releaseCountry": "GB",
      "releaseVersion": "Remastered"
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
        "name": "vinyl"
      }
    ]
  },
  "mediaFormat": "CD",
  "barcode": "0042282778329",
  "catalogNumber": "EMC 4008",
  "musicBrainzReleaseId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "musicBrainzReleaseGroupId": "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e",
  "musicBrainzReleaseArtistId": "c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f",
  "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
  "updatedOnUtc": null,
  "contributors": [
    {
      "contributorId": "0e7b4a6c-5f8d-6b9e-ca0b-3c4d5e6f7a8b",
      "role": "Producer"
    },
    {
      "contributorId": "1a8c5b7d-6a9e-7c0f-db1c-4d5e6f7a8b9c",
      "role": "Engineer"
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
  ],
  "tracks": []
}
```


### Delete Album

```js
DELETE api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/{albumId}
```

```js
200 Ok
```

Deletes the album identified by the route, together with its tracks. An artist must always keep at least one album.


### Get Album Tracks

#### Get Album Tracks Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks
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
    "metadata": {
      "title": "Bohemian Rhapsody",
      "originalTitle": "Bohemian Rhapsody",
      "description": "A song by the British rock band Queen. It was written by Freddie Mercury and originally released on the album A Night at the Opera in 1975.",
      "releaseInfo": { "originalReleaseDate": "1975-10-31", "originalReleaseYear": 1975, "reReleaseDate": null, "reReleaseYear": null, "releaseCountry": "GB", "releaseVersion": "Original" },
      "language": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
      "originalLanguage": { "languageCode": "en", "languageName": "English", "nativeName": "English" },
      "tags": [ { "name": "classic" }, { "name": "epic" } ],
      "genres": [ { "name": "Rock" }, { "name": "Progressive Rock" } ],
      "durationInSeconds": 354,
      "sampleRate": 44100,
      "channels": 2,
      "bitDepth": 16,
      "audioCodec": "FLAC",
      "bitrate": 980
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
    "createdOnUtc": "2025-01-01T12:00:00.0000000Z",
    "updatedOnUtc": null,
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
]
```


### Get Album Tracks Lite

#### Get Album Tracks Lite Request

```js
GET api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/lite?currentPage=1&perPage=10
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




