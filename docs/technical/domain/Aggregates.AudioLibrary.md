- [Lumina Domain](#lumina-domain)
  - [Audio Library Aggregate](#audio-library-aggregate)
    - [Artist](#artist)
    - [Album](#album)
    - [Track](#track)

# [Lumina Domain](#lumina-domain)

## [Audio Library Aggregate](#audio-library-aggregate)

The audio library aggregate is the aggregate root of the `MusicLibrary` aggregate, which models the music of a media library. An artist is the aggregate root, owning its albums, and each album owns its tracks. Albums and tracks carry full metadata: title, description, release information, languages, genres and tags, and the tracks additionally carry the audio characteristics of their file, their moods and their ISRC codes. The credits of the media contributors that make up an artist, or that performed on an album or a track, are carried as `MusicMediaContributorCredit` value objects.

### [Artist](#artist)

```csharp
class Artist
{
    Result<Artist> Create(
        LibraryId libraryId,
        string name,
        Optional<string> website,
        Optional<MusicBrainzId> musicBrainzArtistId,
        List<MusicMediaContributorCredit> credits,
        List<Album> albums);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "libraryId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "name": "Queen",
  "website": "https://www.queenonline.com",
  "musicBrainzArtistId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "credits": [],
  "albums": [
    {
      "value": "00000000-0000-0000-0000-000000000000"
    }
  ]
}
```

### [Album](#album)

```csharp
class Album
{
    Result<Album> Create(
        AlbumMetadata metadata,
        Optional<MusicMediaFormat> mediaFormat,
        Optional<Barcode> barcode,
        Optional<string> catalogNumber,
        Optional<MusicBrainzId> musicBrainzReleaseId,
        Optional<MusicBrainzId> musicBrainzReleaseGroupId,
        Optional<MusicBrainzId> musicBrainzReleaseArtistId,
        List<MusicMediaContributorCredit> credits,
        List<AudioRating> ratings,
        List<Track> tracks);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
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
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "tags": [
      { "name": "classic" },
      { "name": "vinyl" }
    ],
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
    "releaseType": "Album",
    "releaseStatus": "Official",
    "totalDiscs": 1,
    "totalTracks": 12
  },
  "mediaFormat": "CD",
  "barcode": {
    "value": "0042282778329"
  },
  "catalogNumber": "EMC 4008",
  "musicBrainzReleaseId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzReleaseGroupId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzReleaseArtistId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "credits": [],
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
      "value": "00000000-0000-0000-0000-000000000000"
    }
  ]
}
```

### [Track](#track)

```csharp
class Track
{
    Result<Track> Create(
        LibraryId libraryId,
        string path,
        AudioMetadata metadata,
        int trackNumber,
        Optional<int> discNumber,
        List<Isrc> isrcs,
        Optional<string> script,
        Optional<MusicKey> key,
        Optional<int> bpm,
        List<Mood> moods,
        Optional<string> work,
        Optional<MusicBrainzId> musicBrainzRecordingId,
        Optional<MusicBrainzId> musicBrainzTrackId,
        Optional<MusicBrainzId> musicBrainzWorkId,
        List<MusicMediaContributorCredit> credits,
        List<AudioRating> ratings);
}
```

```json
{
  "id": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "libraryId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
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
    "genres": [
      { "name": "Rock" },
      { "name": "Progressive Rock" }
    ],
    "tags": [
      { "name": "classic" },
      { "name": "epic" }
    ],
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
    "durationInSeconds": 354,
    "sampleRate": 44100,
    "channels": 2,
    "bitDepth": 16,
    "audioCodec": "FLAC",
    "bitrate": 980
  },
  "trackNumber": 1,
  "discNumber": 1,
  "isrcs": [
    {
      "value": "GBUM71029604"
    }
  ],
  "script": "Latn",
  "key": "CMajor",
  "bpm": 72,
  "moods": [
    { "name": "dramatic" },
    { "name": "anxious" }
  ],
  "work": "Bohemian Rhapsody",
  "musicBrainzRecordingId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzTrackId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "musicBrainzWorkId": {
    "value": "00000000-0000-0000-0000-000000000000"
  },
  "credits": [],
  "ratings": [
    {
      "value": 4.5,
      "maxValue": 5,
      "source": "MusicBrainz",
      "voteCount": 2345
    }
  ]
}
```
